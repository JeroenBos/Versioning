using Mono.Cecil;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;

namespace Versioning.Tests
{
	public class MissingMethodIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingMethodIssueRaiser().ToSingleton();

		[Test]
		public void MissingMethodIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingMethodIssue>(issues[0]);
		}

		[Test]
		public void NewMethodInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingEventIsReported"/> is the assembly contents are reversed

			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MethodMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MethodMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MethodOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("class A { public void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MissingMethodOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("class A { }"));
			var raiser = new CompatiblityIssueCollector(this.raiser.IssueRaisers.Concat(new[] { new MissingTypeIssueRaiser() }).ToList());

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingTypeIssue>(issues[0]);
		}

		[Test]
		public void ProtectedMethodMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { private void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void InternalMethodMadePrivateIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { internal void m() { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { private void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void ParameterRemovalIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m() { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ParameterAdditionIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(int i, int j) { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}


		[Test]
		public void RefParameterModifierRemovalIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(ref int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(int i) { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void RefParameterModifierAdditionIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(ref int i) { } }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}


		[Test]
		public void ReturnTypeChangeIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public void m(int i) { } }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public int m(int i) => 0; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}
	}
}
