using Mono.Cecil;
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
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void NewFieldInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingFieldIsReported"/> is the assembly contents are reversed

			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void FieldMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void FieldMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void FieldOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("class A { public int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MissingFieldOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("class A { }"));
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
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { private int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}


		[Test]
		public void InternalFieldMadePrivateIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { internal int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { private int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

	}
}
