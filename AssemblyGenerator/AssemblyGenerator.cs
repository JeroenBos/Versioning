using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Versioning
{
	/// <summary>
	/// This generates assemblies on the fly for testing.
	/// </summary>
	public static class AssemblyGenerator
	{
		public static Stream CreateStream(string sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.CSharp8,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(new[] { sourceCode }, assemblyName, out var _, optimizationLevel, languageVersion, references);
		}
		public static Stream CreateStream(string[] sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.CSharp8,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(sourceCode, assemblyName, out var _, optimizationLevel, languageVersion, references);
		}

		private static readonly IReadOnlyCollection<MetadataReference> defaultReferences = new[] {
		  MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
		};

		public static Stream CreateAssembly(
			string sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(new[] { sourceCode }, assemblyName, out var _, optimizationLevel, languageVersion, references);
		}
		public static Stream CreateAssembly(
			string[] sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(sourceCode, assemblyName, out var _, optimizationLevel, languageVersion, references);
		}


		public static Stream CreateAssembly(
			string[] sourceCode,
			string assemblyName,
			out MetadataReference reference,
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default,
			IReadOnlyCollection<MetadataReference>? references = null)

		{
			// parse
			var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: languageVersion);
			var syntaxTrees = sourceCode.Select(sourceCode => CSharpSyntaxTree.ParseText(sourceCode, parseOptions));


			// compile
			var outputKind = sourceCode.Contains(" Main(") ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary;
			var compilationOptions = new CSharpCompilationOptions(outputKind, optimizationLevel: optimizationLevel, allowUnsafe: true);
			Compilation compilation = CSharpCompilation.Create(assemblyName, options: compilationOptions, references: defaultReferences)
			  .AddReferences(references ?? Array.Empty<MetadataReference>())
			  .AddSyntaxTrees(syntaxTrees);

			reference = compilation.ToMetadataReference();

			// emit
			var stream = new MemoryStream();
			var emitResult = compilation.Emit(stream);
			var diagnostics = emitResult.Diagnostics;

			if (emitResult.Success)
			{
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
			throw new Exception("Emitting failed");
		}

		public static AssemblyLoadContext Load(
			string sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default)
		{
			var assemblyStream = CreateAssembly(new[] { sourceCode }, assemblyName, optimizationLevel, languageVersion);
			AssemblyLoadContext context = new TemporaryAssemblyLoadContext();
			context.LoadFromStream(assemblyStream);
			return context;
		}


		public static AssemblyLoadContext LoadAssemblyWithReference(
			string referencedAssemblySourceCode,
			string sourceCode,
			string referencedAssemblyName = "defaultReferencedAssemblyName",
			string assemblyName = "defaultAssemblyName")
		{
			AssemblyLoadContext context = new TemporaryAssemblyLoadContext();

			var referencedStream = CreateAssembly(new[] { referencedAssemblySourceCode }, referencedAssemblyName, out MetadataReference reference);
			var assemblyStream = CreateAssembly(new[] { sourceCode }, assemblyName, references: new[] { reference });

			context.LoadFromStream(referencedStream);
			context.LoadFromStream(assemblyStream);
			return context;
		}


		/// <summary>
		/// Compiles the source code against an assembly containing <paramref name="referencedAssemblySourceCode"/>,
		/// but loads the assembly containing <paramref name="referencedAssemblySourceCodev2"/>.
		/// </summary>
		public static AssemblyLoadContext LoadAssemblyWithReferenceAgainstDifferentVersion(
			string referencedAssemblySourceCode,
			string referencedAssemblySourceCodev2,
			string sourceCode,
			string referencedAssemblyName = "defaultReferencedAssemblyName",
			string assemblyName = "defaultAssemblyName")
		{
			return LoadAssemblyWithReferenceAgainstDifferentVersion(referencedAssemblySourceCode, referencedAssemblySourceCodev2, sourceCode, out var _, referencedAssemblyName, assemblyName);
		}

		/// <summary>
		/// Compiles the source code against an assembly containing <paramref name="referencedAssemblySourceCode"/>,
		/// but loads the assembly containing <paramref name="referencedAssemblySourceCodev2"/>.
		/// </summary>
		public static AssemblyLoadContext LoadAssemblyWithReferenceAgainstDifferentVersion(
			string referencedAssemblySourceCode,
			string referencedAssemblySourceCodev2,
			string sourceCode,
			out (AssemblyDefinition dependencyV1, AssemblyDefinition dependencyV2, AssemblyDefinition main) assemblyDefinitions,
			string referencedAssemblyName = "defaultReferencedAssemblyName",
			string assemblyName = "defaultAssemblyName")
		{
			AssemblyLoadContext context = new TemporaryAssemblyLoadContext();

			const string v2Attribute = "[assembly: System.Reflection.AssemblyVersion(\"2.0.0\")]";

			var dependencyV1 = CreateAssembly(new[] { referencedAssemblySourceCode }, referencedAssemblyName, out MetadataReference reference);
			var dependencyV2 = CreateAssembly(new[] { referencedAssemblySourceCodev2, v2Attribute }, referencedAssemblyName);
			var main = CreateAssembly(new[] { sourceCode }, assemblyName, references: new[] { reference });

			context.LoadFromStream(dependencyV2);
			context.LoadFromStream(main);
			dependencyV2.Position = 0;
			main.Position = 0;

			assemblyDefinitions = (AssemblyDefinition.ReadAssembly(dependencyV1), AssemblyDefinition.ReadAssembly(dependencyV2), AssemblyDefinition.ReadAssembly(main));
			return context;
		}
	}
}
