using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Versioning.Tests
{
	public class AssemblyGeneratorTests
	{
		[Test]
		public void CanCreateEmptyAssembly()
		{
			// Just checking that it doesn't throw
			Stream assembly = AssemblyGenerator.CreateAssembly("", "assemblyname", out var _);
			Assert.IsTrue(assembly.Length != 0);
		}
		[Test]
		public void CanUseTypeFromSeparatelyLoadedAssembly()
		{
			/// This also checks that internally the overload <see cref="Load(AssemblyName)"/> isn't used when loading from stream
			var assemblyContext = AssemblyGenerator.Load("public class A { }", "assemblyname");
			var assembly = assemblyContext.Assemblies.First();

			Assert.AreEqual("A", assembly.GetTypes()[0].Name);
		}

	}
}