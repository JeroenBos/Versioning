using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Versioning.DiffDetector;
using Versioning.UsageDetector;
using IssueDetector = Versioning.UsageDetector.UsageDetector;

namespace Versioning.CLI
{
	public static class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 2 && args.Length != 3)
			{
				Console.WriteLine("Expected 2 or 3 arguments:");
				Console.WriteLine("- 2: Paths to an assembly and an assembly of higher version. Potential compatibility issues are listed.");
				Console.WriteLine("- 3: Paths to an assembly, a dependency assembly and a dependency assembly of higher version.");
				Console.WriteLine("     Compatibility issues are listed when the main assembly would encounter the higher version at runtime. ");
				Console.ReadKey();
				return;
			}

			var assemblies = args.Select(arg =>
			{
				try
				{
					return AssemblyDefinition.ReadAssembly(arg);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}
			}).ToList();

			if (assemblies.Any(a => a == null))
			{
				Console.ReadKey();
				return;
			}

			using var consoleWriter = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
			Console.SetOut(consoleWriter);

			if (args.Length == 3)
			{
				ListDetectedIssues(assemblies[0], assemblies[1], assemblies[2], consoleWriter);
			}
			else
			{
				ListPotentialIssues(assemblies[0], assemblies[1], consoleWriter);
			}

			Console.ReadKey();
		}

		private static void ListDetectedIssues(AssemblyDefinition main, AssemblyDefinition dependency, AssemblyDefinition dependencyHigherVersion, TextWriter writer)
		{
			var issueCollector = CompatiblityIssueCollector.Default;
			var detectedIssues = IssueDetector.DetectCompatibilityIssues(issueCollector, main, dependency, dependencyHigherVersion)
											  .ToList();

			detectedIssues.WriteTo(writer, main.Name, dependency.Name, dependencyHigherVersion.Name);
		}

		private static void ListPotentialIssues(AssemblyDefinition assembly, AssemblyDefinition assemblyHigherVersion, TextWriter writer)
		{
			var issueCollector = CompatiblityIssueCollector.Default;
			var potentialIssues = issueCollector.GetCompatibilityIssuesBetween(assembly, assemblyHigherVersion)
												.ToList();

			potentialIssues.WriteTo(writer, assembly.Name, assemblyHigherVersion.Name);
		}



		/// <summary>
		/// Writes the detected compatibility issue as a report to the specified writer.
		/// </summary>
		public static void WriteTo(
			this IReadOnlyList<IDetectedCompatibilityIssue> detectedIssues,
			TextWriter writer,
			AssemblyNameReference assemblyName,
			AssemblyNameReference dependencyName,
			AssemblyNameReference dependencyHigherVersionName)
		{
			writer.WriteLine($"Detected {detectedIssues.Count} issues{(detectedIssues.Count == 0 ? '.' : ':')}");
			writer.WriteLine();

			foreach (var detectedIssue in detectedIssues)
			{
				writer.WriteLine(detectedIssue.ToDisplayString());
			}

			writer.WriteLine();
			writer.WriteLine("Done");
		}


		/// <summary>
		/// Writes the potential compatibility issue as a report to the specified writer.
		/// </summary>
		public static void WriteTo(
			this IReadOnlyList<ICompatibilityIssue> potentialIssues,
			TextWriter writer,
			AssemblyNameReference assemblyName,
			AssemblyNameReference assemblyHigherVersionName)
		{
			if (assemblyName.Name != assemblyHigherVersionName.Name)
			{
				writer.WriteLine($"Warning: the same assembly but different versions should be specified, but got '{assemblyName.Name}' and '{assemblyHigherVersionName.Name}'.");
				writer.WriteLine();
			}
			else if (assemblyName.Version >= assemblyHigherVersionName.Version)
			{
				writer.WriteLine($"Warning: the second argument did not point to an assembly of higher version.");
				writer.WriteLine();
			}

			writer.WriteLine($"Detected {potentialIssues.Count} potential compatibility issues");
			writer.WriteLine($"between assembly {assemblyName.Name} versions {assemblyName.Version} and {assemblyHigherVersionName.Version}{(potentialIssues.Count == 0 ? '.' : ':')}");
			writer.WriteLine();

			foreach (var issueGroup in potentialIssues.GroupBy(d => d.ToHeaderDisplayString()))
			{
				writer.WriteLine(issueGroup.Key); // the header

				var sortedIssueGroup = issueGroup.Select(issue => issue.ToDisplayString()).OrderBy(_ => _);
				foreach (string issue in sortedIssueGroup)
				{
					writer.Write("- ");
					writer.WriteLine(issue);
				}
				writer.WriteLine();
			}

			writer.WriteLine("Done");
		}
	}
}
