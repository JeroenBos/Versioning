using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;

namespace Versioning.Tests
{
	public class MissingPropertyOrAccessorIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingPropertyOrAccessorIssueRaiser().ToSingleton();

		[Test]
		public void MissingPropertyIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P => 0; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Get, ((MissingPropertyOrAccessorIssue)issues[0]).MissingAccessors);
		}

		[Test]
		public void MissingAutoimplementedPropertyIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Get, ((MissingPropertyOrAccessorIssue)issues[0]).MissingAccessors);
		}

		[Test]
		public void MissingGetterIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; set; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int P { set { } } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Get, ((MissingPropertyOrAccessorIssue)issues[0]).MissingAccessors);
		}
		[Test]
		public void MissingSetterIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; set; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int P { get; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingPropertyOrAccessorIssue)issues[0]).MissingAccessors);
		}

		[Test]
		public void NewPropertyInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingFieldIsReported"/> is the assembly contents are reversed

			// arrange
			Assembly a = AssemblyGenerator.Load("").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int P { get; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void PropertyMadePrivateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { int P { get; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void PropertyMadeProtectedIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { protected int P { get; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void PropertyOnMissingTypeIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { public int P { get; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}
	}
}
