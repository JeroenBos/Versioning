_Summary:_

Detects compatibility issues in different versions of .NET dependency assemblies, and their relevance to a dependent assembly.

The project was started due to [this blog post](https://codeblog.jonskeet.uk/2019/10/25/options-for-nets-versioning-issues/) by Jon Skeet.

## Description
Suppose you have a project that depends on assembly `Name=myDependency, Version=1.0` 
but you want to know whether it is _runtime_ compatible with a higher version, `2.0`, say. 

This project provides a solution which lists all type and member signatures that changed between the versions.
Moreover, it can also lists all those differences that are relevant to your assembly.
For example if a type is removed but is referenced nowhere in your project, then running against the higher versioned dependency should still be fine.

Note: in this context a 'compatibility issue' means code that, when run, would throw a binding failure exception, e.g. `TypeLoadException`.
This entails that only changes to signatures (as opposed to bodies) of members or types are considered to be compatibility issues:
changed behavior of a piece of code whose surface remains unchanged is not considered here.

## Example usage
This project can be consumed as CLI or as library. 
The CLI accepts 2 or 3 paths as arguments:
- 2 args: Paths to an assembly and an assembly of higher version. Potential compatibility issues are listed.
- 3 args: Paths to an assembly, a dependency assembly and a dependency assembly of higher version.
     Compatibility issues are listed when the main assembly would encounter the higher version at runtime. 

Library usages is discussed now.
## Project overview

Let's sketch a quick overview of the code hierarchy and responsibilities in this project.

- `AssemblyGenerator`, which is used for testing purposes only, contains helper methods to generate assemblies on the fly using Roslyn, where they are scanned and run.
- `Versioning` is the project dedicated to solving the first part of the problem, 
   namely to list all differences between two assembly versions.
   The suggested method for external consumption is 
   `CompatiblityIssueCollector.Default.GetCompatibilityIssuesBetween(assembly, otherAssembly)`.
   The result is a list of implementations of the interface `ICompatibilityIssue`.
- `UsageDetector` is the project dedicated to solving the second part of the problem, 
namely to filter all differences listed by `Versioning` on relevance to a third assembly, mostly referred to as the 'main assembly'.
The suggested method for external consumption is 
   `UsageDetector.DetectCompatibilityIssues(collector, mainAssembly, dependencyAssembly, dependencyAssemblyHigherVersion)`.
The result is a list of implementations of the interface `IDetectedCompatibilityIssue`.
- The CLI project merely wraps this for easy consumption, and can stringify `ICompatibilityIssue` and `IDetectedCompatibilityIssue` objects.

