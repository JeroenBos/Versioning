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
			if (args.Length != 3)
			{
				Console.WriteLine("Expected 3 argument:, paths to main assembly, dependent assembly and dependent assembly of higher version");
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

			var (main, dependency, dependencyHigherVersion) = (assemblies[0], assemblies[1], assemblies[2]);


			var issueCollector = CompatiblityIssueCollector.MissingMembersIssueCollector;
			var detectedIssues = IssueDetector.DetectCompatibilityIssues(issueCollector, main, dependency, dependencyHigherVersion)
				                              .ToList();

			Console.WriteLine($"Detected {detectedIssues.Count} issues:");
			foreach(var detectedIssue in detectedIssues)
			{
				Console.WriteLine(detectedIssue.ToDisplayString());
			}
			Console.WriteLine("Done");
			Console.ReadKey();
		}
	}
}
