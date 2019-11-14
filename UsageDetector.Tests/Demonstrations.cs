using Microsoft.CodeAnalysis;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Versioning.UsageDetector.Tests
{
	// This class provides examples of runtime binding exceptions being thrown whenever an issues is raised
	class Demonstrations : TestHelper
	{
		[Test]
		public async Task RunMain()
		{
			const string sourceCode_DependencyV1 = @"";
			const string sourceCode_DependencyV2 = @"";
			const string sourceCode_Main = @"class P { static void Main(string[] args) { } }";

			var (entryPoint, detectedIssues) = DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main, otherDependencies: Framework4_7_2);

			Assert.AreEqual(0, detectedIssues.Count);
			var (exitCode, _, _) = await entryPoint!();
			Assert.AreEqual(0, exitCode);
		}


		[Test]
		public async Task Nodatime_IClock_Now()
		{
			const string sourceCode_Main = @"
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NodaTime; 

class C 
{
	static void Main(string[] args)
	{
		var i = SystemClock.Instance.Now;
	}
}";

			string nodatime1_4_7Path_3_5 = Path.Combine(PackagesDirectory, "NodaTime.1.4.7", "lib", "net35-Client", "NodaTime.dll");
			string nodatime2_4_7Path = Path.Combine(PackagesDirectory, "NodaTime.2.4.7", "lib", "net45", "NodaTime.dll");

			var (entryPoint, detectedIssues) = DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(
				dependencyReference: MetadataReference.CreateFromFile(nodatime1_4_7Path_3_5),
				runtimeDependencyPath: nodatime2_4_7Path,
				sourceCode_Main: new[] { sourceCode_Main },
				otherDependencies: Framework4_7_2
			);
			Assert.AreEqual(0, detectedIssues.Count);
			var (_, _, errorOutput) = await entryPoint!();
			Assert.IsTrue(errorOutput.Contains("System.IO.FileLoadException: Could not load file or assembly 'NodaTime, Version=1.4"));
			Assert.IsTrue(errorOutput.Contains("The located assembly's manifest definition does not match the assembly reference."));
		}
	}
}
