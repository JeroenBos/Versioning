using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Versioning.UsageDetector.Tests
{
	public class Exploratory
	{
		[Test]
		public void ListMethodReferenceFromFieldInitializer()
		{
			const string source = @"class C { int i = System.Math.Abs(0); }";
			var assembly = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(source));
			var allMethodCalls = UsageDetector.GetAllMemberReferences(assembly)
										      .OfType<MethodReference>()
										      .OrderBy(r => r.Name)
										      .ToList();

			Assert.AreEqual(2, allMethodCalls.Count);
			Assert.AreEqual(".ctor", allMethodCalls[0].Name);
			Assert.AreEqual("Abs", allMethodCalls[1].Name);
		}

		[Test]
		public void ListMethodReferenceFromLocalFunction()
		{
			const string source = @"
class C {
	void M()
	{
		void m()
		{
			System.Math.Abs(0);
		}
	}
}";
			var assembly = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(source));
			var allMethodCalls = UsageDetector.GetAllMemberReferences(assembly)
											  .OfType<MethodReference>()
											  .OrderBy(r => r.Name)
											  .ToList();

			Assert.AreEqual(2, allMethodCalls.Count);
			Assert.AreEqual(".ctor", allMethodCalls[0].Name);
			Assert.AreEqual("Abs", allMethodCalls[1].Name);
		}

		[Test]
		public void ListFieldReference()
		{
			const string source = @"
class C {
	static int i;
    void m()
    {
        int j = i;
    }
}";
			var assembly = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(source));
			var refs = UsageDetector.GetAllMemberReferences(assembly)
									.OfType<FieldReference>()
									.OrderBy(r => r.Name)
									.ToList();

			Assert.AreEqual(1, refs.Count);
			Assert.AreEqual("i", refs[0].Name);
		}

		[Test]
		public void ListTypeofReference()
		{
			const string source = @"
class C {
	System.Type t = typeof(object);
}";
			var assembly = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(source));
			var refs = UsageDetector.GetAllMemberReferences(assembly)
									.OfType<TypeReference>()
									.OrderBy(r => r.Name)
									.ToList();

			Assert.AreEqual(1, refs.Count);
			Assert.AreEqual("Object", refs[0].Name);
		}


		[Test]
		public void ListTypeReferences()
		{
			const string source = @"
class C {
	static C c;
}";
			var assembly = AssemblyDefinition.ReadAssembly(AssemblyGenerator.CreateAssembly(source));
			var refs = UsageDetector.GetAllTypeReferences(assembly)
									.OrderBy(r => r.Name)
									.ToList();

			Assert.AreEqual(11, refs.Count);
		}
	}
}