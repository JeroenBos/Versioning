using Microsoft.CodeAnalysis;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Versioning.DiffDetector.Issues;

namespace Versioning.UsageDetector.Tests
{
	// This class provides examples of runtime binding exceptions being thrown whenever an issues is raised too
	class Demonstrations : TestHelper
	{
		/// <summary>
		/// This test demonstrates that an compatibility issue is detected when moving from NodaTime v1.4.7 to v2.4.7,
		/// and that this is in fact an issue, as demonstrated by the thrown runtime exception.
		/// </summary>
		[Test]
		public async Task NodaTime_IClock_Now()
		{
			// arrange
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

			// act
			var (entryPoint, detectedIssues) = DetectIssuesAndPrepareWithReferenceAgainstDifferentVersion(
				compileTimeDependency: MetadataReference.CreateFromFile(NodaTime_1_4_7Path_3_5),
				runtimeDependency: MetadataReference.CreateFromFile(NodaTime_2_4_7Path_4_5),
				sourceCode_Main: new[] { sourceCode_Main },
				otherDependencies: _NETFramework4_5_Or_Higher
			);
			var (_, _, errorOutput) = await entryPoint!();

			// assert
			Assert.AreEqual(1, detectedIssues.Count);
			Assert.IsAssignableFrom<MissingMemberIssue>(detectedIssues[0].Issue);
			Assert.AreEqual(((MissingMemberIssue)detectedIssues[0].Issue).MissingMember.Name, "Now");

			Assert.IsTrue(errorOutput.Contains("System.IO.FileLoadException: Could not load file or assembly 'NodaTime, Version=1.4"));
			Assert.IsTrue(errorOutput.Contains("The located assembly's manifest definition does not match the assembly reference."));

			// write report to disk
			CreateReport(detectedIssues,
				new AssemblyNameReference("NodaTime", new Version(1, 4, 7)),
				new AssemblyNameReference("NodaTime", new Version(2, 4, 7)));
		}
	}
}

