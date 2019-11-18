using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.DiffDetector.IssueRaisers;
using Versioning.DiffDetector.Issues;
using Mono.Cecil;

namespace Versioning.DiffDetector.Tests
{
	public class AccessibilityReducedTests
	{
		private CompatiblityIssueCollector raiser = new MemberAccessibilityReducedIssueRaiser().ToSingleton();

		[Test]
		public void EventMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void EventMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ProtectedEventMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void InternalEventMadePrivateIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { internal event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void FieldMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void FieldMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ProtectedFieldMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void InternalFieldMadePrivateIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { internal int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MethodMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MethodMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void NestedStructMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { public struct S { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected struct S { }}"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("S", ((MemberAccessibilityReducedIssue)issues[0]).Member.Name);
		}

		[Test]
		public void NestedProtectedStructMadeProtectedInternalIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected struct S { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected internal struct S { }}"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MemberInInternalTypeReducesAccessibilityIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { public int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void ProtectedMemberThatChangesToPrivateProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private protected int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MemberAccessibilityReducedIssue>(issues[0]);
		}

		[Test]
		public void InternalProtectedMemberThatChangesToPrivateProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { internal protected int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private protected int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MemberAccessibilityReducedIssue>(issues[0]);
		}

		[Test]
		public void PrivateProtectedMemberThatChangesToProtectedIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private protected int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { protected int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void PrivateProtectedMemberThatChangesToInternalProtectedIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { private protected int i; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("public class A { internal protected int i; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void PrivateProtectedChangeInInternalClassIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { private protected int i;  internal protected int j; public int k; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly("class A { internal protected int i; private protected int j;  private int k; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}


	}
}
