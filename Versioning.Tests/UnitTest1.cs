using NUnit.Framework;
using System.IO;

namespace Versioning.Tests
{
	public class Tests
	{
		[Test]
		public void CanCreateEmptyAssembly()
		{
			// Just checking that it doesn't throw
			Stream assembly = AssemblyGenerator.CreateAssembly("", "assemblyname");
			Assert.IsTrue(assembly.Length != 0);
		}
	}
}