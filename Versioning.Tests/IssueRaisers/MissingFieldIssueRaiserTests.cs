using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;

namespace Versioning.Tests
{
	public class MissingFieldIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingFieldIssueRaiser().ToSingleton();

		[Test]
		public void MissingFieldIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int i; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void NewFieldInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingFieldIsReported"/> is the assembly contents are reversed

			// arrange
			Assembly a = AssemblyGenerator.Load("").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int i; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void FieldMadePrivateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int i; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { int i; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void FieldMadeProtectedIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int i; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { protected int i; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void FieldOnMissingTypeIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int i; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { public int i; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MissingFieldOnMissingTypeIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int i; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { }").Assemblies.First();
			var raiser = new CompatiblityIssueCollector(this.raiser.IssueRaisers.Concat(new[] { new MissingTypeIssueRaiser() }).ToList());

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingTypeIssue>(issues[0]);
		}

		[Test]
		public void ProtectedFieldMadePrivateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { protected int i; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { private int i; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}


		[Test]
		public void InternalFieldMadePrivateIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { internal int i; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { private int i; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

	}
}
