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
			if(args.Length != 2 && args.Length != 3)
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
				ListIssues(assemblies[0], assemblies[1], assemblies[2]);
			}
			else
			{
				ListDifferences(assemblies[0], assemblies[1]);
			}

			Console.WriteLine("Done");
			Console.ReadKey();
		}

		private static void ListIssues(AssemblyDefinition main, AssemblyDefinition dependency, AssemblyDefinition dependencyHigherVersion)
		{
			var issueCollector = CompatiblityIssueCollector.MissingMembersIssueCollector;
			var detectedIssues = IssueDetector.DetectCompatibilityIssues(issueCollector, main, dependency, dependencyHigherVersion)
											  .ToList();

			Console.WriteLine($"Detected {detectedIssues.Count} issues:");
			foreach (var detectedIssue in detectedIssues)
			{
				Console.WriteLine(detectedIssue.ToDisplayString());
			}
		}

		private static void ListDifferences(AssemblyDefinition assembly, AssemblyDefinition assemblyHigherVersion)
		{
			var issueCollector = CompatiblityIssueCollector.MissingMembersIssueCollector;
			var potentialIssues = issueCollector.GetCompatibilityIssuesBetween(assembly, assemblyHigherVersion)
				                                .ToList();

			Console.WriteLine($"Detected {potentialIssues.Count} potential compatibility issues:");
			foreach (var potentialIssue in potentialIssues)
			{
				Console.WriteLine(potentialIssue.ToDisplayString());
			}
		}

	}
}
