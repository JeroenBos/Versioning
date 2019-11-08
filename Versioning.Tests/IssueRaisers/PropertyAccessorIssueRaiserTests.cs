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
	public class PropertyAccessorIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = CompatiblityIssueCollector.Default;

		[Test]
		public void MissingPropertyIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P => 0; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingMemberIssue>(issues[0]);
		}

		[Test]
		public void MissingAutoimplementedPropertyIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingMemberIssue>(issues[0]);
		}

		[Test]
		public void MissingGetterIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; set; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { set { } } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Get, ((MissingAccessorIssue)issues[0]).Accessor);
		}
		[Test]
		public void MissingSetterIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; set; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
		}

		[Test]
		public void NewPropertyInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingFieldIsReported"/> is the assembly contents are reversed

			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void PropertyMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { int P { get; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void PropertyMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected int P { get; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void PropertyOnMissingTypeIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("class A { public int P { get; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MemberAccessibilityReducedIssue>(issues[0]);
			Assert.AreEqual("A", ((MemberAccessibilityReducedIssue)issues[0]).Member.Name);
		}


		[Test]
		public void AccessorMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; set; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; private set; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
		}

		[Test]
		public void AccessorMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; set; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; protected set; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
		}


		[Test]
		public void AccessorMadePrivateFromProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; protected set; } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int P { get; private set; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsTrue(((MissingMemberIssue)issues[0]).MissingMember.Name.StartsWith("set"));
		}

		[Test]
		public void MissingIndexerIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int this[object obj] => 0; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingMemberIssue>(issues[0]);
		}
		public class A { public int this[object obj] { get => 0;  set { } } }

		[Test]
		public void MissingIndexerGetterIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int this[object obj] { get => 0;  set { } } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int this[object obj] { set { } } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Get, ((MissingAccessorIssue)issues[0]).Accessor);
		}
		[Test]
		public void MissingIndexerSetterIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int this[object obj] { get => 0;  set { } } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int this[object obj] { get => 0; } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
		}
		// TODO: indexer overload testing

	}
}
