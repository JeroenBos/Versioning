using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;

namespace Versioning.Tests
{
	public class MissingTypeIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingTypeIssueRaiser().ToSingleton();

		[Test]
		public void MissingTypeIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingTypeIssue>(issues[0]);
			Assert.AreEqual("A", ((MissingTypeIssue)issues[0]).MissingType.FullName);
		}


		[Test]
		public void NewTypeInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingTypeIsReported"/> is the assembly contents are reversed
			
			// arrange
			Assembly a = AssemblyGenerator.Load("").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}


		[Test]
		public void TypeMadeInternalIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}
	}
}
