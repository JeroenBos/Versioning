using Microsoft.CodeAnalysis;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Versioning.UsageDetector.Tests
{
	// This class provides examples of runtime binding exceptions being thrown whenever an issues is raised
	class Demonstrations : TestHelper
	{
		[Test]
		public void RunMain()
		{
			const string sourceCode_DependencyV1 = @"";
			const string sourceCode_DependencyV2 = @"";
			const string sourceCode_Main = @"class P { static void Main(string[] args) { } }";

			var (entryPoint, detectedIssues) = DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main);

			Assert.AreEqual(0, detectedIssues.Count);
			Assert.DoesNotThrowAsync(entryPoint);
		}


		[Test]
		public void Nodatime_IClock_Now()
		{
			const string sourceCode_Main = @"
using System;
using System.Diagnostics;
using NodaTime; 

public class C 
{
	public static void Main(string[] args)
	{
		Trace.WriteLine(""In dependency"");
		throw new System.Exception(""Hi"");
	}
}";
			string nodatime1_4_7Path_3_5 = Path.Combine(PackagesDirectory, "NodaTime.1.4.7", "lib", "net35-Client", "NodaTime.dll");
			string nodatime2_4_7Path = Path.Combine(PackagesDirectory, "NodaTime.2.4.7", "lib", "netstandard2.0", "NodaTime.dll");

			var (entryPoint, detectedIssues) = DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(
				dependencyReference: MetadataReference.CreateFromFile(nodatime1_4_7Path_3_5),
				runtimeDependencyPath: nodatime2_4_7Path,
				sourceCode_Main: sourceCode_Main,
				otherDependencies: framework
			);
			Assert.AreEqual(0, detectedIssues.Count);
			Assert.DoesNotThrowAsync(entryPoint);
		}
	}
}
