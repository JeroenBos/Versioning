using Microsoft.CodeAnalysis;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TargetDotNetFrameworkVersion = Microsoft.Build.Utilities.TargetDotNetFrameworkVersion;
using ToolLocationHelper = Microsoft.Build.Utilities.ToolLocationHelper;
using Versioning.CLI;


namespace Versioning.UsageDetector.Tests
{
	class TestHelper
	{
		[OneTimeSetUp]
		public static async Task EnsureDependenciesInstalled()
		{
			if (!File.Exists(NodaTime_1_4_7Path_3_5))
			{
				await NugetInstall("NodaTime", "1.4.7");
			}
			if (!File.Exists(NodaTime_2_4_7Path_4_5))
			{
				await NugetInstall("NodaTime", "2.4.7");
			}
		}

		/// <summary>
		/// Gets the path to the global nuget package repository on this machine.
		/// </summary>
		public static string PackagesDirectory
		{
			get
			{
				var path = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
						?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
				return Path.GetFullPath(path);
			}
		}
		/// <summary>
		/// Gets the path to the CLI/Results directory to store test results.
		/// </summary>
		public static string TestResultsPath
		{
			get => Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "CLI", "Results"));
		}

		public static string NodaTime_1_4_7Path_3_5 => Path.Combine(PackagesDirectory, "NodaTime", "1.4.7", "lib", "net35-Client", "NodaTime.dll");
		public static string NodaTime_2_4_7Path_4_5 => Path.Combine(PackagesDirectory, "NodaTime", "2.4.7", "lib", "net45", "NodaTime.dll");
		public static NamedMetadataReference NodaTime_1_4_7Reference_3_5
		{
			get => (MetadataReference.CreateFromFile(NodaTime_1_4_7Path_3_5), new AssemblyNameReference("NodaTime", new Version(1, 4, 7)));
		}
		public static NamedMetadataReference NodaTime_2_4_7Reference_4_5
		{
			get => (MetadataReference.CreateFromFile(NodaTime_2_4_7Path_4_5), new AssemblyNameReference("NodaTime", new Version(2, 4, 7)));
		}


		public static string PathToNETFrameworkOver4_5
		{
			get
			{
				for (TargetDotNetFrameworkVersion version = TargetDotNetFrameworkVersion.Version472; version >= TargetDotNetFrameworkVersion.Version45; version--)
				{
					string result = ToolLocationHelper.GetPathToDotNetFrameworkReferenceAssemblies(version);
					if (result != null)
						return result;
				}
				throw new Exception("No .NET Framework reference assemblies with version in the range [4.5, 4.7.2] could be found");
			}
		}

		public static PortableExecutableReference[] _NETFramework4_5_Or_Higher => Framework(PathToNETFrameworkOver4_5);

		/// <summary>
		/// Gets a minimal selection of meta data references given a reference assemblies directory.
		/// </summary>
		internal static PortableExecutableReference[] Framework(string referenceAssembliesPath)
		{
			var assemblies = new[] { "mscorlib", "System", "System.Core", "Facades/netstandard", "Facades/System.Runtime" }
				.Select(name => Path.Combine(referenceAssembliesPath, name + ".dll"))
				.Where(File.Exists)
				.Select(path => MetadataReference.CreateFromFile(path))
				.ToArray();
			return assemblies;
		}

		/// <summary>
		/// Installs the specified package into the global nuget packages repository on this machine.
		/// </summary>
		public static Task<int> NugetInstall(string packageName, string? version = null)
		{
			string versionArg = version != null ? " -Version " + version : "";
			string pathArg = " -OutputDirectory " + "\"" + PackagesDirectory + "\"";

			return ProcessExtensions.StartIndependentlyInvisiblyAsync("nuget.exe", $"install {packageName} {versionArg} {pathArg}");
		}

		/// <summary>
		/// A helper method for testing, which 
		/// - compiles the specified source code with specified dependencies, 
		/// - collects all potential compatibility issues for the case when the dependency would be updated to the different version, 
		/// - detects whether those potential issues could possibly be an actual issue given the (just-compiled) assembly,
		/// - creates a delegate wrapping the entry point of the resulting assembly running against a different version of the dependency
		///   (the idea is that the reported issues are triggered at runtime, to verify the issue wasn't a false positive). 
		/// - returns the issues with delegate to trigger the issues.
		/// </summary>
		/// <param name="compileTimeDependency"> A reference to the dependency version against which the source code is compiled. </param>
		/// <param name="runtimeDependency"> A reference to the dependency version resolved at runtime in the returned delegate. </param>
		/// <param name="sourceCode_Main"> The source code files to compile. </param>
		/// <param name="issueRaiser"> The compatibility issue detector. Specify null to use the default. </param>
		/// <param name="otherDependencies"> Other references that the source code depends upon, like .NET framework.
		/// Specify nothing to reference the current .NET Core version. </param>
		public static EntryPointPlusIssues DetectIssuesAndPrepareWithReferenceAgainstDifferentVersion(
			PortableExecutableReference compileTimeDependency,
			PortableExecutableReference runtimeDependency,
			string[] sourceCode_Main,
			CompatiblityIssueCollector? issueRaiser = null,
			params PortableExecutableReference[] otherDependencies)
		{
			var entryPoint = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(compileTimeDependency, runtimeDependency.FilePath, sourceCode_Main, otherDependencies, out var assemblyDefinitions);

			var (dependencyV1, dependencyV2, main) = assemblyDefinitions;
			var issues = UsageDetector.DetectCompatibilityIssues(issueRaiser ?? CompatiblityIssueCollector.Default, main, dependencyV1, dependencyV2).ToList();

			return (entryPoint, issues);
		}

		/// <summary>
		/// Loads the main assembly built against the dependency v1, but runtime loads dependency v2, and returns the main assembly's entry point as action, if any.
		/// Also returns raised issues.
		/// </summary>
		/// <param name="issueRaiser"> Specify null to use the default issue collector. </param>
		public static EntryPointPlusIssues DetectIssuesAndPrepareWithReferenceAgainstDifferentVersion(
			string sourceCode_DependencyV1,
			string sourceCode_DependencyV2,
			string sourceCode_Main,
			CompatiblityIssueCollector? issueRaiser = null,
			params PortableExecutableReference[] otherDependencies)
		{
			ProcessDelegate? entryPoint = AssemblyGenerator.CreateAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main, out var assemblyDefinitions, otherReferences: otherDependencies);

			var (dependencyV1, dependencyV2, main) = assemblyDefinitions;
			var issues = UsageDetector.DetectCompatibilityIssues(issueRaiser ?? CompatiblityIssueCollector.Default, main, dependencyV1, dependencyV2).ToList();

			return (entryPoint, issues);
		}

		public static void CreateReport(
			IReadOnlyList<IDetectedCompatibilityIssue> detectedIssues,
			AssemblyNameReference dependencyName,
			AssemblyNameReference dependencyHigherVersionName,
			AssemblyNameReference? assemblyName = null,
			[CallerMemberName] string fileName = "")
		{
			assemblyName ??= new AssemblyNameReference("defaultAssemblyName", new Version(2, 0, 0));

			try // reporting isn't crucial for the test itself, hence the try-catch
			{
				using var file = File.Open(Path.Combine(TestResultsPath, fileName + ".txt"), FileMode.Create);
				detectedIssues.WriteTo(new StreamWriter(file) { AutoFlush = true }, assemblyName, dependencyName, dependencyHigherVersionName);
			}
			catch { }
		}
	}
}
