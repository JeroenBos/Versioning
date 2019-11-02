using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;

namespace Versioning
{
	public static class AssemblyGenerator
	{
		private static readonly IReadOnlyCollection<MetadataReference> _references = new[] {
		  MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
		};

		public static MemoryStream CreateAssembly(
			string sourceCode,
			string assemblyName,
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.CSharp8)

		{
			// parse
			var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: languageVersion);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, parseOptions);


			// compile
			var compilationOptions = new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: optimizationLevel,
				allowUnsafe: true);
			Compilation compilation =  CSharpCompilation.Create(assemblyName, options: compilationOptions, references: _references)
			  .AddReferences(_references)
			  .AddSyntaxTrees(syntaxTree);

			// emit
			var stream = new MemoryStream();
			var emitResult = compilation.Emit(stream);

			if (emitResult.Success)
			{
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
			throw new Exception("Emitting failed. ");
		}
	}
}
