using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;

namespace Versioning
{
	/// <summary>
	/// This generates assemblies on the fly for testing.
	/// </summary>
	public static class AssemblyGenerator
	{
		private static readonly IReadOnlyCollection<MetadataReference> defaultReferences = new[] {
		  MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
		};


		public static Stream CreateAssembly(
			string sourceCode,
			string assemblyName,
			out MetadataReference reference,
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default,
			IReadOnlyCollection<MetadataReference>? references = null)

		{
			// parse
			var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: languageVersion);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, parseOptions);


			// compile
			var compilationOptions = new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: optimizationLevel,
				allowUnsafe: true);
			Compilation compilation = CSharpCompilation.Create(assemblyName, options: compilationOptions, references: defaultReferences)
			  .AddReferences(references ?? Array.Empty<MetadataReference>())
			  .AddSyntaxTrees(syntaxTree);

			reference = compilation.ToMetadataReference();

			// emit
			var stream = new MemoryStream();
			var emitResult = compilation.Emit(stream);
			var diagnostics = compilation.GetDeclarationDiagnostics();

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
			var assemblyStream = CreateAssembly(sourceCode, assemblyName, out var _, optimizationLevel, languageVersion);
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

			var referencedStream = CreateAssembly(referencedAssemblySourceCode, referencedAssemblyName, out MetadataReference reference);
			var assemblyStream = CreateAssembly(sourceCode, assemblyName, out var _, references: new[] { reference });

			context.LoadFromStream(referencedStream);
			context.LoadFromStream(assemblyStream);
			return context;
		}

		/// <summary>
		/// Compiles the source code against an assembly containing <paramref name="referencedAssemblySourceCode"/>,
		/// but loads the assembly containing <paramref name="referencedAssemblySourceCodev2"/>.
		/// </summary>
		public static AssemblyLoadContext LoadAssemblyWithReferenceAgainstDifferenceVersion(
			string referencedAssemblySourceCode,
			string referencedAssemblySourceCodev2,
			string sourceCode,
			string referencedAssemblyName = "defaultReferencedAssemblyName",
			string assemblyName = "defaultAssemblyName")
		{
			AssemblyLoadContext context = new TemporaryAssemblyLoadContext();
			
			referencedAssemblySourceCodev2 = "[assembly: System.Reflection.AssemblyVersion(\"2.0.0\")]" + referencedAssemblySourceCodev2;

			CreateAssembly(referencedAssemblySourceCode, referencedAssemblyName, out MetadataReference reference);
			var referencedAssemblyRuntime = CreateAssembly(referencedAssemblySourceCodev2, referencedAssemblyName, out var _);
			var assemblyStream = CreateAssembly(sourceCode, assemblyName, out var _, references: new[] { reference });

			context.LoadFromStream(referencedAssemblyRuntime);
			context.LoadFromStream(assemblyStream);
			return context;
		}
	}
}
