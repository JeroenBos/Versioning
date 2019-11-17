using Microsoft.CodeAnalysis;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Versioning.Issues;

namespace Versioning.UsageDetector.Tests
{
	// This class provides examples of runtime binding exceptions being thrown whenever an issues is raised
	class Demonstrations : TestHelper
	{
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
			var (entryPoint, detectedIssues) = DetectIssuesAndLoadAssemblyWithReferenceAgainstDifferentVersion(
				dependencyReference: MetadataReference.CreateFromFile(NodaTime_1_4_7Path_3_5),
				runtimeDependencyPath: NodaTime_2_4_7Path_4_5,
				sourceCode_Main: new[] { sourceCode_Main },
				otherDependencies: Framework4_7_2
			);
			Assert.AreEqual(1, detectedIssues.Count);
			Assert.IsAssignableFrom<MissingMemberIssue>(detectedIssues[0].Issue);
			Assert.AreEqual(((MissingMemberIssue)detectedIssues[0].Issue).MissingMember.Name, "Now");

			var (_, _, errorOutput) = await entryPoint!();
			Assert.IsTrue(errorOutput.Contains("System.IO.FileLoadException: Could not load file or assembly 'NodaTime, Version=1.4"));
			Assert.IsTrue(errorOutput.Contains("The located assembly's manifest definition does not match the assembly reference."));
		}
	}
}
