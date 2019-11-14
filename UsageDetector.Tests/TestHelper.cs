using Microsoft.CodeAnalysis;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Versioning.UsageDetector.Tests
{
	class TestHelper
	{
		public static string PackagesDirectory => Path.GetFullPath("../../../Packages");

		public static PortableExecutableReference[] Framework4_7_2 => Framework("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2/");
		internal static PortableExecutableReference[] Framework(string path)
		{
			var assemblies = new[] { "mscorlib", "System", "System.Core", "Facades/netstandard", "Facades/System.Runtime" }
				.Select(name => Path.Combine(path, name + ".dll"))
				.Select(path => MetadataReference.CreateFromFile(path))
				.ToArray();
			return assemblies;
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
