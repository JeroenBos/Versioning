using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.IssueRaisers;
using Versioning.Issues;

namespace Versioning.Tests
{
	public class MissingEventIssueRaiserTests
	{
		private CompatiblityIssueCollector raiser = new MissingEventIssueRaiser().ToSingleton();
		
		[Test]
		public void MissingEventIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public event System.Action a; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
			Assert.IsAssignableFrom<MissingEventIssue>(issues[0]);
		}

		[Test]
		public void NewEventInNewAssemblyIsNotReported()
		{
			/// the difference with <see cref="MissingEventIsReported"/> is the assembly contents are reversed

			// arrange
			Assembly a = AssemblyGenerator.Load("").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { public event System.Action a; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}

		[Test]
		public void EventMadePrivateIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public event System.Action a; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { event System.Action a; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void EventMadeProtectedIsReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public event System.Action a; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("public class A { protected event System.Action a; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(1, issues.Count);
		}

		[Test]
		public void EventOnMissingTypeIsNotReported()
		{
			// arrange
			Assembly a = AssemblyGenerator.Load("public class A { public event System.Action a; }").Assemblies.First();
			Assembly b = AssemblyGenerator.Load("class A { public event System.Action a; }").Assemblies.First();

			// act
			var issues = raiser.GetCompatibilityIssuesBetween(a, b).ToList();

			Assert.AreEqual(0, issues.Count);
		}
	}
}
