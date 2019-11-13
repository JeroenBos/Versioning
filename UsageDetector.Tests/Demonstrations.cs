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
		public static TestDelegate? LoadAssemblyWithReferenceAgainstDifferenceVersion(string sourceCode_DependencyV1, string sourceCode_DependencyV2, string sourceCode_Main)
		{
			var assemblies = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferenceVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main).Assemblies.ToList();

			var mainAssembly = assemblies[1];
			if (mainAssembly.EntryPoint == null)
				return null;
			return () => mainAssembly.EntryPoint.Invoke(null, new object?[] { null });
		}

		[Test]
		public void RunMain()
		{
			const string sourceCode_DependencyV1 = @"";
			const string sourceCode_DependencyV2 = @"";
			const string sourceCode_Main = @"public class Program { public static void Main(string[] args) { } }";

			TestDelegate entryPoint = LoadAssemblyWithReferenceAgainstDifferenceVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main)!;

			entryPoint();
		}
	}
}
