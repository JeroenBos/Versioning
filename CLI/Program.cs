using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IssueDetector = Versioning.UsageDetector.UsageDetector;

namespace Versioning.CLI
{
	class Program
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


			if (args.Length == 3)
			{
				ListDetectedIssues(assemblies[0], assemblies[1], assemblies[2]);
			}
			else
			{
				ListPotentialIssues(assemblies[0], assemblies[1]);
			}

			Console.ReadKey();
		}

		private static void ListDetectedIssues(AssemblyDefinition main, AssemblyDefinition dependency, AssemblyDefinition dependencyHigherVersion)
		{
			var issueCollector = CompatiblityIssueCollector.Default;
			var detectedIssues = IssueDetector.DetectCompatibilityIssues(issueCollector, main, dependency, dependencyHigherVersion)
											  .ToList();

			Console.WriteLine($"Detected {detectedIssues.Count} issues:");
			foreach (var detectedIssue in detectedIssues)
			{
				Console.WriteLine(detectedIssue.ToDisplayString());
			}

			if (detectedIssues.Count != 0)
			{
				Console.WriteLine("Done");
			}
		}

		private static void ListPotentialIssues(AssemblyDefinition assembly, AssemblyDefinition assemblyHigherVersion)
		{
			if (assembly.Name.Name != assemblyHigherVersion.Name.Name)
			{
				Console.WriteLine($"Warning: the same assembly but different versions should be specified, but got '{assembly.Name.Name}' and '{assemblyHigherVersion.Name.Name}'.");
			}
			else if (assembly.Name.Version >= assemblyHigherVersion.Name.Version)
			{
				Console.WriteLine($"Warning: the second argument did not point to an assembly of higher version.");
			}

			var issueCollector = CompatiblityIssueCollector.Default;
			var potentialIssues = issueCollector.GetCompatibilityIssuesBetween(assembly, assemblyHigherVersion)
												.ToList();
			Console.WriteLine($"Detected {potentialIssues.Count} potential compatibility issues");
			Console.WriteLine($"between assembly {assembly.Name.Name} versions {assembly.Name.Version} and {assemblyHigherVersion.Name.Version}:");
			Console.WriteLine();

			foreach (var issueGroup in potentialIssues.GroupBy(d => d.ToHeaderDisplayString()))
			{
				Console.WriteLine(issueGroup.Key); // the header

				var sortedIssueGroup = issueGroup.Select(issue => issue.ToDisplayString()).OrderBy(_ => _);
				foreach (string issue in sortedIssueGroup)
				{
					Console.Write("- ");
					Console.WriteLine(issue);
				}
				Console.WriteLine();
			}

			if (potentialIssues.Count != 0)
			{
				Console.WriteLine("Done");
			}
		}

	}
}
