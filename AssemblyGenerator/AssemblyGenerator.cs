using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Versioning
{
	/// <summary>
	/// This generates assemblies on the fly for testing.
	/// </summary>
	public static class AssemblyGenerator
	{
		/// <summary>
		/// Creates an in-memory assembly containing the specified source code.
		/// </summary>
		public static Stream CreateAssembly(
			string sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(new[] { sourceCode }, assemblyName, references, out var _, out var _, optimizationLevel, languageVersion);
		}

		/// <summary>
		/// Creates an in-memory assembly containing the specified source code.
		/// </summary>
		public static Stream CreateAssembly(
			string[] sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(sourceCode, assemblyName, references, out var _, out var _, optimizationLevel, languageVersion);
		}

		/// <summary>
		/// Creates an in-memory assembly containing the specified source code.
		/// </summary>
		internal static Stream CreateAssembly(
			string[] sourceCode,
			string assemblyName,
			IReadOnlyCollection<MetadataReference>? references,
			out MetadataReference reference,
			out OutputKind outputKind,
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.Default)
		{
			// parse
			var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: languageVersion);
			var syntaxTrees = sourceCode.Select(sourceCode => CSharpSyntaxTree.ParseText(sourceCode, parseOptions)).ToList();


			// compile
			outputKind = syntaxTrees.Any(HasEntryPoint) ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary;
			var compilationOptions = new CSharpCompilationOptions(outputKind, optimizationLevel: optimizationLevel, allowUnsafe: true);
			Compilation compilation = CSharpCompilation.Create(assemblyName, options: compilationOptions)
			  .AddReferences(ConcatReferencesHelper(references))
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

			static bool HasEntryPoint(SyntaxTree sourceCode) => sourceCode
				.GetRoot()
				.DescendantNodesAndSelf()
				.OfType<MethodDeclarationSyntax>()
				.Any(m => m.Identifier.ValueText == "Main"); // will do for now
		}

		/// <summary>
		/// Compiles the source code against an assembly containing <paramref name="referencedAssemblySourceCode"/>,
		/// but loads the assembly containing <paramref name="referencedAssemblySourceCodev2"/>.
		/// </summary>
		public static ProcessDelegate? CreateAssemblyWithReferenceAgainstDifferentVersion(
			string referencedAssemblySourceCode,
			string referencedAssemblySourceCodev2,
			string sourceCode,
			string referencedAssemblyName = "defaultReferencedAssemblyName",
			string assemblyName = "defaultAssemblyName",
			IReadOnlyList<PortableExecutableReference>? otherReferences = null)
		{
			return CreateAssemblyWithReferenceAgainstDifferentVersion(referencedAssemblySourceCode, referencedAssemblySourceCodev2, sourceCode, out var _, referencedAssemblyName, assemblyName, otherReferences: otherReferences);
		}

		/// <summary>
		/// Compiles the source code against an assembly containing <paramref name="referencedAssemblySourceCode"/>,
		/// but loads the assembly containing <paramref name="referencedAssemblySourceCodev2"/>.
		/// </summary>
		public static ProcessDelegate? CreateAssemblyWithReferenceAgainstDifferentVersion(
			string referencedAssemblySourceCode,
			string referencedAssemblySourceCodev2,
			string sourceCode,
			out (AssemblyDefinition dependencyV1, AssemblyDefinition dependencyV2, AssemblyDefinition main) assemblyDefinitions,
			string referencedAssemblyName = "defaultReferencedAssemblyName",
			string assemblyName = "defaultAssemblyName",
			IReadOnlyList<PortableExecutableReference>? otherReferences = null)
		{
			const string v2Attribute = "[assembly: System.Reflection.AssemblyVersion(\"2.0.0\")]";
			
			var dependencyV1 = CreateAssembly(new[] { referencedAssemblySourceCode }, referencedAssemblyName, null, out MetadataReference reference, out var _);
			var dependencyV2 = CreateAssembly(new[] { referencedAssemblySourceCodev2, v2Attribute }, referencedAssemblyName);

			var references = ConcatReferencesHelper(otherReferences, reference);
			var main = CreateAssembly(new[] { sourceCode }, assemblyName, references, out var _, out var outputKind);

			assemblyDefinitions = (AssemblyDefinition.ReadAssembly(dependencyV1), AssemblyDefinition.ReadAssembly(dependencyV2), AssemblyDefinition.ReadAssembly(main));

			return CopyToTempDirectory((assemblyName, main), 
				                       outputKind,
				                       Array.Empty<string>(), 
				                       (referencedAssemblyName, dependencyV2));
		}


		/// <summary>
		/// Compiles the source code against an assembly containing <paramref name="referencedAssemblySourceCode"/>,
		/// but loads the assembly containing <paramref name="referencedAssemblySourceCodev2"/>.
		/// </summary>
		public static ProcessDelegate? LoadAssemblyWithReferenceAgainstDifferentVersion(
			PortableExecutableReference dependencyReference,
			string dependencyToBeLoadedPath,
			string[] sourceCode,
			IReadOnlyList<PortableExecutableReference> otherReferences,
			out (AssemblyDefinition dependencyV1, AssemblyDefinition dependencyV2, AssemblyDefinition main) assemblyDefinitions,
			string assemblyName = "defaultAssemblyName")
		{
			var references = ConcatReferencesHelper(otherReferences, dependencyReference);
			var assemblyStream = CreateAssembly(sourceCode, assemblyName, references, out var _, out var outputKind);

			assemblyDefinitions = (AssemblyDefinition.ReadAssembly(dependencyReference.FilePath),
				                   AssemblyDefinition.ReadAssembly(dependencyToBeLoadedPath),
				                   AssemblyDefinition.ReadAssembly(assemblyStream));

			return CopyToTempDirectory((assemblyName, assemblyStream),
									   outputKind,
									   otherReferences.Select(r => r.FilePath).Concat(new[] { dependencyToBeLoadedPath }));
		}

		private static ProcessDelegate? CopyToTempDirectory(
			NamedAssemblyStream mainAssembly, 
			OutputKind outputKind, 
			IEnumerable<string> dependenciesPaths, 
			NamedAssemblyStream dependency = default)
		{
			var tempDirPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString());
			Trace.WriteLine("Temp dir: " + tempDirPath);
			var dir = Directory.CreateDirectory(tempDirPath);
			var mainAssemblyPath = Path.Combine(dir.FullName, mainAssembly.Name + (outputKind == OutputKind.ConsoleApplication ? ".exe" : ".dll"));

			WriteAllTo(mainAssembly.Stream, mainAssemblyPath);
			if (dependency.Stream != null)
			{
				WriteAllTo(dependency.Stream, Path.Combine(dir.FullName, dependency.Name + ".dll"));
			}

			foreach (string dependencyPath in dependenciesPaths)
			{
				File.Copy(dependencyPath, Path.Combine(dir.FullName, Path.GetFileName(dependencyPath)));
			}


			if (outputKind == OutputKind.ConsoleApplication)
				return () => new ProcessStartInfo(mainAssemblyPath).WaitForExitAndReadOutputAsync();
			return null;

			static void WriteAllTo(Stream stream, string path)
			{
				stream.Seek(0, SeekOrigin.Begin);
				using var fileStream = File.Create(path);
				stream.CopyTo(fileStream);
				stream.Seek(0, SeekOrigin.Begin);
			}
		}

		/// <summary>
		/// Replaces the <paramref name="otherReferences"/> by the default references if empty, and appends the <paramref name="explicitReferences"/>.
		/// </summary>
		private static IReadOnlyList<MetadataReference> ConcatReferencesHelper(
			IReadOnlyCollection<MetadataReference>? otherReferences,
			params MetadataReference[] explicitReferences)
		{
			if(otherReferences == null || otherReferences.Count == 0)
				otherReferences = new[] { MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location) };

			return otherReferences.Concat(explicitReferences).ToList();
		}
	}
}
