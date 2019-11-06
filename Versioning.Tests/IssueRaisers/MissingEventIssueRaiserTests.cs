using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;
using Mono.Cecil;

namespace Versioning.Tests
{
	public class MissingEventIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingEventIssueRaiser().ToSingleton();
		
		[Test]
		public void MissingEventIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingEventIssue>(issues[0]);
		}

		[Test]
		public void NewEventInNewAssemblyIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream(""));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void EventMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void EventMadeProtectedIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void EventOnMissingTypeIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { public event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("class A { public event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void ProtectedEventMadePrivateIsReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { protected event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { private event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}


		[Test]
		public void InternalEventMadePrivateIsNotReported()
		{
			// arrange
			var a = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { internal event System.Action a; }"));
			var b = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateStream("public class A { private event System.Action a; }"));

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}
	}
}
