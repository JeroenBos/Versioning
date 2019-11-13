using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Versioning.UsageDetector.Tests
{
	// This class provides examples of runtime binding exceptions being thrown whenever an issues is raised
	class Demonstrations
	{
		/// <summary>
		/// Loads the main assembly built against the dependency v1, but runtime loads dependency v2, and returns the main assembly's entry point as action, if any.
		/// </summary>
		public static TestDelegate? LoadAssemblyWithReferenceAgainstDifferentVersion(
			string sourceCode_DependencyV1,
			string sourceCode_DependencyV2,
			string sourceCode_Main,
			out (AssemblyDefinition dependencyV1, AssemblyDefinition dependencyV2, AssemblyDefinition main) assemblyDefinitions)
		{
			var assemblies = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main, out assemblyDefinitions).Assemblies.ToList();

			var mainAssembly = assemblies[1];
			if (mainAssembly.EntryPoint == null)
				return null;
			return () => mainAssembly.EntryPoint.Invoke(null, new object?[] { null });
		}

		/// <summary>
		/// Loads the main assembly built against the dependency v1, but runtime loads dependency v2, and returns the main assembly's entry point as action, if any.
		/// Also returns raised issues.
		/// </summary>
		/// <param name="issueRaiser"> Specify null to use the default issue collector. </param>
		public static (TestDelegate? entryPoint, IReadOnlyList<IDetectedCompatibilityIssue> issues) DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(
			string sourceCode_DependencyV1,
			string sourceCode_DependencyV2,
			string sourceCode_Main,
			CompatiblityIssueCollector? issueRaiser = null)
		{
			TestDelegate? entryPoint = LoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main, out var assemblyDefinitions);
			var (dependencyV1, dependencyV2, main) = assemblyDefinitions;

			var issues = UsageDetector.DetectCompatibilityIssues(issueRaiser ?? CompatiblityIssueCollector.Default, main, dependencyV1, dependencyV2).ToList();

			return (entryPoint, issues);
		}

		[Test]
		public void RunMain()
		{
			const string sourceCode_DependencyV1 = @"";
			const string sourceCode_DependencyV2 = @"";
			const string sourceCode_Main = @"class P { static void Main(string[] args) { } }";

			var (entryPoint, detectedIssues) = DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main);

			Assert.AreEqual(0, detectedIssues.Count);
			Assert.DoesNotThrow(entryPoint);
		}
	}
}
