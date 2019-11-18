using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Versioning.DiffDetector.IssueRaisers;
using Versioning.DiffDetector.Issues;

namespace Versioning.DiffDetector.Tests
{
	public class MissingMemberTests
	{
		private CompatiblityIssueCollector raiser = new MissingMemberIssueRaiser().ToSingleton();

		[Test]
		public void MissingFieldIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void NewFieldInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingFieldIsReported"/> is the assembly contents are reversed

			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MissingEventIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingMemberIssue>(issues[0]);
		}

		[Test]
		public void NewEventInNewAssemblyIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void EventOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { public event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void FieldOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { public int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MissingFieldOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingMemberIssue>(issues[0]);
		}

		[Test]
		public void MissingMethodIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MethodDefinition>(((MissingMemberIssue)issues[0]).MissingMember);
		}

		[Test]
		public void NewMethodInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingEventIsReported"/> is the assembly contents are reversed

			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void ParameterRemovalIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ParameterAdditionIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(int i, int j) { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void RefParameterModifierRemovalIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(ref int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(int i) { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void RefParameterModifierAdditionIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(ref int i) { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ReturnTypeChangeIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public int m(int i) => 0; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MissingClassIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(""));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("A", ((MissingMemberIssue)issues[0]).MissingMember.FullName);
		}

		[Test]
		public void NewClassInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingTypeIsReported"/> is the assembly contents are reversed

			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MissingNestedDelegateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public delegate void D(); }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("D", ((MissingMemberIssue)issues[0]).MissingMember.Name);
		}

		[Test]
		public void MemberRemovalInInternalTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}
	}
}
