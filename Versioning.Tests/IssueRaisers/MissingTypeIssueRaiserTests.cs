﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;

namespace Versioning.Tests
{
	public class MissingTypeIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingTypeIssueRaiser().ToSingleton();

		[Test]
		public void MissingTypeIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("").Assemblies.First();

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
			Assembly a = AssemblyGenerator.Load("").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void TypeMadeInternalIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void MissingDelegateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public delegate void D();").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void ChangingFromClassToValueTypeIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public struct A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);

		}

		[Test]
		public void MissingNestedDelegateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public delegate void D(); }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("D", ((MissingTypeIssue)issues[0]).MissingType.Name);
		}

		[Test]
		public void NestedStructMadeInternalIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public struct S { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { internal struct S { }}").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("S", ((MissingTypeIssue)issues[0]).MissingType.Name);
		}

		[Test]
		public void NestedStructMadeProtectedIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public struct S { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { protected struct S { }}").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.AreEqual("S", ((MissingTypeIssue)issues[0]).MissingType.Name);
		}

		[Test]
		public void NestedProtectedStructMadeProtectedInternalIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { protected struct S { } }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { protected internal struct S { }}").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}
	}
}
