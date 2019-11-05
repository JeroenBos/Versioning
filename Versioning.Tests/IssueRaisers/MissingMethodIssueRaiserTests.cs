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
			Assembly a = AssemblyGenerator.Load("public class A { public void m() { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

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
			Assembly a = AssemblyGenerator.Load("").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public void m() { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MethodMadePrivateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m() { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { void m() { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MethodMadeProtectedIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m() { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { protected void m() { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MethodOnMissingTypeIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m() { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { public void m() { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void MissingMethodOnMissingTypeIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m() { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void ProtectedMethodMadePrivateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { protected void m() { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { private void m() { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void InternalMethodMadePrivateIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { internal void m() { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { private void m() { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void ParameterRemovalIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m(int i) { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public void m() { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ParameterAdditionIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m(int i) { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public void m(int i, int j) { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}


		[Test]
		public void RefParameterModifierRemovalIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m(ref int i) { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public void m(int i) { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void RefParameterModifierAdditionIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m(int i) { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public void m(ref int i) { } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}


		[Test]
		public void ReturnTypeChangeIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public void m(int i) { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int m(int i) => 0; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}
	}
}
