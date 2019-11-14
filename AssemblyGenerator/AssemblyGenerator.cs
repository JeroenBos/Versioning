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
		public static Stream CreateStream(string sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.CSharp7_3,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(new[] { sourceCode }, assemblyName, out var _, optimizationLevel, languageVersion, references);
		}
		public static Stream CreateStream(string[] sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.CSharp7_3,
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
			LanguageVersion languageVersion = LanguageVersion.CSharp7_3,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(new[] { sourceCode }, assemblyName, out var _, optimizationLevel, languageVersion, references);
		}
		public static Stream CreateAssembly(
			string[] sourceCode,
			string assemblyName = "defaultAssemblyName",
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.CSharp7_3,
			IReadOnlyCollection<MetadataReference>? references = null)
		{
			return CreateAssembly(sourceCode, assemblyName, out var _, optimizationLevel, languageVersion, references);
		}

		public static bool HasEntryPoint(SyntaxTree sourceCode)
		{
			return sourceCode.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().Any(m => m.Identifier.ValueText == "Main"); // will do for now
		}
		public static Stream CreateAssembly(
			string[] sourceCode,
			string assemblyName,
			out MetadataReference reference,
			OptimizationLevel optimizationLevel = OptimizationLevel.Release,
			LanguageVersion languageVersion = LanguageVersion.CSharp7_3,
			IReadOnlyCollection<MetadataReference>? references = null)

		{
			// parse
			var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: languageVersion);
			var syntaxTrees = sourceCode.Select(sourceCode => CSharpSyntaxTree.ParseText(sourceCode, parseOptions)).ToList();


			// compile
			var outputKind = syntaxTrees.Any(HasEntryPoint) ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary;
			var compilationOptions = new CSharpCompilationOptions(outputKind, optimizationLevel: optimizationLevel, allowUnsafe: true);
			Compilation compilation = CSharpCompilation.Create(assemblyName, options: compilationOptions)
			  .AddReferences(references ?? defaultReferences)
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
			throw new SystemException("Emitting failed");
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


		/// <summary>
		/// Compiles the source code against an assembly containing <paramref name="referencedAssemblySourceCode"/>,
		/// but loads the assembly containing <paramref name="referencedAssemblySourceCodev2"/>.
		/// </summary>
		public static Func<Task> LoadAssemblyWithReferenceAgainstDifferentVersion(
			PortableExecutableReference dependencyReference,
			string dependencyToBeLoadedPath,
			string sourceCode,
			out (AssemblyDefinition dependencyV1, AssemblyDefinition dependencyV2, AssemblyDefinition main) assemblyDefinitions,
			IReadOnlyList<PortableExecutableReference>? otherReferences = null,
			string assemblyName = "defaultAssemblyName")
		{
			otherReferences ??= Array.Empty<PortableExecutableReference>();

			var references = otherReferences.Concat(new[] { dependencyReference }).ToList();
			var main = CreateAssembly(sourceCode, assemblyName, references: references);

			var tempDirPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString());
			Trace.WriteLine("Temp dir: " + tempDirPath);
			var dir = Directory.CreateDirectory(tempDirPath);
			var mainAssemblyPath = Path.Combine(dir.FullName, assemblyName + ".dll");
			using (var fileStream = File.Create(mainAssemblyPath))
			{
				main.CopyTo(fileStream);
				main.Seek(0, SeekOrigin.Begin);
			}

			foreach (string dependencyPath in otherReferences.Select(r => r.FilePath).Concat(new[] { dependencyToBeLoadedPath }))
			{
				File.Copy(dependencyPath, dir.FullName + Path.GetFileName(dependencyPath));
			}

			assemblyDefinitions = (AssemblyDefinition.ReadAssembly(dependencyReference.FilePath), AssemblyDefinition.ReadAssembly(dependencyToBeLoadedPath), AssemblyDefinition.ReadAssembly(main));

			return () => Process.Start(mainAssemblyPath).WaitForExitAsync();
		}
	}
}
