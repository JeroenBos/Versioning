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
			Assert.IsAssignableFrom<MissingPropertyIssue>(issues[0]);
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
			Assert.IsAssignableFrom<MissingPropertyIssue>(issues[0]);
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
			Assert.AreEqual(PropertyAccessor.Get, ((MissingAccessorIssue)issues[0]).Accessor);
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
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
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


		[Test]
		public void AccessorMadePrivateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; set; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int P { get; private set; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
		}

		[Test]
		public void AccessorMadeProtectedIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; set; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int P { get; protected set; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
		}


		[Test]
		public void AccessorMadePrivateFromProtectedIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int P { get; protected set; } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int P { get; private set; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsTrue(((MissingMethodIssue)issues[0]).MissingMethod.Name.StartsWith("set"));
		}

		[Test]
		public void MissingIndexerIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int this[object obj] => 0; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingPropertyIssue>(issues[0]);
		}
		public class A { public int this[object obj] { get => 0;  set { } } }

		[Test]
		public void MissingIndexerGetterIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int this[object obj] { get => 0;  set { } } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int this[object obj] { set { } } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Get, ((MissingAccessorIssue)issues[0]).Accessor);
		}
		[Test]
		public void MissingIndexerSetterIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public int this[object obj] { get => 0;  set { } } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public int this[object obj] { get => 0; } }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual(PropertyAccessor.Set, ((MissingAccessorIssue)issues[0]).Accessor);
		}
		// TODO: indexer overload testing

	}
}
