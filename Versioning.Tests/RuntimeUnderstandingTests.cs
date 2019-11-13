using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace Versioning.Tests
{
	/// <summary>
	/// I don't know a whole lot about how exactly the runtime resolves assembly items regarding assembly versions, so I'll just check it out a bit.
	/// </summary>
	public class RuntimeUnderstandingTests
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

			// runtime test:
			var b = Activator.CreateInstance(assemblies[1].GetTypes()[0]);
			Assert.IsNotNull(b);
			Assert.AreEqual("B", b.GetType().Name);
		}

		[Test]
		public void LoadAssemblyWithReferenceAgainstDifferenceVersion()
		{
			const string sourceCode_DependencyV1 = @"public class A { }";
			const string sourceCode_DependencyV2 = @"public class A { public int i; }";
			const string sourceCode_Main = @"public class B : A { }";

			var assemblies = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main).Assemblies.ToList();
			Assert.AreEqual("2.0.0.0", assemblies[0].GetName().Version.ToString(), "The assembly with higher number was not loaded");
			// the metadata reference still says 0.0.0.0 though:
			Assert.AreEqual("0.0.0.0", assemblies[1].GetReferencedAssemblies()[1].Version.ToString());
			// just making super-duper sure that the new version is actually referenced:
			Assert.AreEqual(1, assemblies[1].GetTypes()[0].GetFields().Length);
		}

		[Test]
		public void CanLoadAssemblyWithDifferentReferencedVersion()
		{
			const string sourceCode_DependencyV1 = @"public class A { }";
			const string sourceCode_DependencyV2 = @"public class A { }";
			const string sourceCode_Main = @"public class B : A { }";

			var assemblies = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main).Assemblies.ToList();
			var b = Activator.CreateInstance(assemblies[1].GetTypes()[0]);
			Assert.AreEqual("B", b.GetType().Name);
		}

		[Test]
		public void CanAddAbstractModifierAndStillBindType()
		{
			// just verifing my expections:
			// this test shows that you can in fact in some circumstances add the abstract modifier to a type
			// and an assembly depending on it still accepts the abstract type

			const string sourceCode_DependencyV1 = @"public class A { }";
			const string sourceCode_DependencyV2 = @"public abstract class A { }";
			const string sourceCode_Main = @"public class B : A { }";

			var assemblies = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main).Assemblies.ToList();
			var b = Activator.CreateInstance(assemblies[1].GetTypes()[0]);
			Assert.AreEqual("B", b.GetType().Name);
		}

		[Test]
		public void BindingFailsWhenMethodIsMadeAbstract()
		{
			const string sourceCode_DependencyV1 = @"public abstract class A { public void M() { } }";
			const string sourceCode_DependencyV2 = @"public abstract class A { public abstract void M();}";
			const string sourceCode_Main = @"public class B : A { }";
			var assemblies = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main).Assemblies.ToList();

			Assert.Throws<ReflectionTypeLoadException>(() => Activator.CreateInstance(assemblies[1].GetTypes()[0]));
		}

		[Test]
		public void BindingFailsWhenParameterChanges()
		{
			const string sourceCode_DependencyV1 = @"public abstract class A { public void M(int i) { } }";
			const string sourceCode_DependencyV2 = @"public abstract class A { public void M(uint i) { } }";
			const string sourceCode_Main = @"public class B : A { public void F() { base.M(0); }}";
			var assemblies = AssemblyGenerator.LoadAssemblyWithReferenceAgainstDifferentVersion(sourceCode_DependencyV1, sourceCode_DependencyV2, sourceCode_Main).Assemblies.ToList();

			dynamic b = Activator.CreateInstance(assemblies[1].GetTypes()[0]);
			Assert.Throws<MissingMethodException>(() => b.F());
		}

	}
}
