using Microsoft.CodeAnalysis;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TargetDotNetFrameworkVersion = Microsoft.Build.Utilities.TargetDotNetFrameworkVersion;
using ToolLocationHelper = Microsoft.Build.Utilities.ToolLocationHelper;


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

		public static string PackagesDirectory
		{
			get
			{
				var path = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
						?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
				return Path.GetFullPath(path);
			}
		}
		public static string NodaTime_1_4_7Path_3_5 => Path.Combine(PackagesDirectory, "NodaTime", "1.4.7", "lib", "net35-Client", "NodaTime.dll");
		public static string NodaTime_2_4_7Path_4_5 => Path.Combine(PackagesDirectory, "NodaTime", "2.4.7", "lib", "net45", "NodaTime.dll");

		public static string Framework_4_7_2Path => ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version472);
		public static string Latest_NETFrameworkOver4_5Path
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

		public static PortableExecutableReference[] _NETFramework4_5_Or_Higher => Framework(Latest_NETFrameworkOver4_5Path);
		internal static PortableExecutableReference[] Framework(string path)
		{
			var assemblies = new[] { "mscorlib", "System", "System.Core", "Facades/netstandard", "Facades/System.Runtime" }
				.Select(name => Path.Combine(path, name + ".dll"))
				.Where(File.Exists)
				.Select(path => MetadataReference.CreateFromFile(path))
				.ToArray();
			return assemblies;
		}

		public static Task<int> NugetInstall(string packageName, string? version = null)
		{
			string versionArg = version != null ? " -Version " + version : "";
			string pathArg = " -OutputDirectory " + "\"" + PackagesDirectory + "\"";

			return ProcessExtensions.StartIndependentlyInvisiblyAsync("nuget.exe", $"install {packageName} {versionArg} {pathArg}");
		}

		public static (ProcessDelegate? entryPoint, IReadOnlyList<IDetectedCompatibilityIssue> issues) DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(
			PortableExecutableReference dependencyReference,
			string runtimeDependencyPath,
			string[] sourceCode_Main,
			CompatiblityIssueCollector? issueRaiser = null,
			params PortableExecutableReference[] otherDependencies)
		{
			var entryPoint = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(dependencyReference, runtimeDependencyPath, sourceCode_Main, out var assemblyDefinitions, otherDependencies);

			var (dependencyV1, dependencyV2, main) = assemblyDefinitions;
			var issues = UsageDetector.DetectCompatibilityIssues(issueRaiser ?? CompatiblityIssueCollector.Default, main, dependencyV1, dependencyV2).ToList();

			return (entryPoint, issues);
		}

		/// <summary>
		/// Loads the main assembly built against the dependency v1, but runtime loads dependency v2, and returns the main assembly's entry point as action, if any.
		/// Also returns raised issues.
		/// </summary>
		/// <param name="issueRaiser"> Specify null to use the default issue collector. </param>
		public static (ProcessDelegate? entryPoint, IReadOnlyList<IDetectedCompatibilityIssue> issues) DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(
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
	}
}
