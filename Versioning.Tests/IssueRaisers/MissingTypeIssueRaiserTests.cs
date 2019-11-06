using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;
using Mono.Cecil;

namespace Versioning.Tests
{
	public class MissingTypeIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingTypeIssueRaiser().ToSingleton();

		[Test]
		public void MissingTypeIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream(""));

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
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void TypeMadeInternalIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MissingDelegateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public delegate void D();"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream(""));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ChangingFromClassToValueTypeIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public struct A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);

		}

		[Test]
		public void MissingNestedDelegateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public delegate void D(); }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("D", ((MissingTypeIssue)issues[0]).MissingType.Name);
		}

		[Test]
		public void NestedStructMadeInternalIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public struct S { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { internal struct S { }}"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("S", ((MissingTypeIssue)issues[0]).MissingType.Name);
		}

		[Test]
		public void NestedStructMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public struct S { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected struct S { }}"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("S", ((MissingTypeIssue)issues[0]).MissingType.Name);
		}

		[Test]
		public void NestedProtectedStructMadeProtectedInternalIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected struct S { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected internal struct S { }}"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}
	}
}
