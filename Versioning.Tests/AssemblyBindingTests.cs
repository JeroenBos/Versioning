using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Versioning.Tests
{
	/// <summary>
	/// I don't know a whole lot about how exactly the runtime resolves assembly items regarding assembly versions, so I'll just check it out a bit.
	/// </summary>
	public class A
	{
		[Test]
		public void LoadAssemblyWithReference()
		{
			const string referencedSourceCode = @"public class A { }";
			const string sourceCode = @"public class B { }";
			var assemblies = AssemblyGenerator.LoadAssemblyWithReference(referencedSourceCode, sourceCode).Assemblies.ToList();
			Assert.AreEqual(2, assemblies.Count);
		}

		[Test]
		public void LoadAssemblyUsingTypeFromReference()
		{
			const string referencedSourceCode = @"public class A { }";
			const string sourceCode = @"public class B : A { }";
			var assemblies = AssemblyGenerator.LoadAssemblyWithReference(referencedSourceCode, sourceCode).Assemblies.ToList();
			Assert.AreEqual(2, assemblies.Count);
		}

	}
}