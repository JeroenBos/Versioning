using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.UsageDetector
{
	public static class UsageDetector
	{
		/// <summary>
		/// This is the entry point for solving the original problem: 
		/// - Detect all type and member references to a particular assembly.
		/// - Detect all differences between two versions of that assembly.
		/// - Find the intersection.
		/// </summary>
		public static IEnumerable<DetectedCompatibilityIssue> DetectCompatibilityIssues(
			CompatiblityIssueCollector collector,
			AssemblyDefinition main,
			AssemblyDefinition dependency,
			AssemblyDefinition dependencyHigherVersion)
		{
			var references = GetAllReferences(main)
							   .Where(reference => reference.RefersIn(dependency))
							   .ToList();

			var issues = collector.GetCompatibilityIssuesBetween(dependency, dependencyHigherVersion)
								  .ToList();

			return from issue in issues
				   let locations = DetectIssue(issue, references).ToList()
				   where locations.Count != 0
				   select new DetectedCompatibilityIssue(issue, locations);
		}

		/// <summary>
		/// Gets all <see cref="MemberReference"/>s in the specified assembly. 
		/// </summary>
		public static IEnumerable<MemberReference> GetAllReferences(AssemblyDefinition assembly)
		{
			// the current implementation is an abominable hack, but, in the interest of completeness, 
			// this seems easier and safer than for me to manually list all places in an assembly where member references can occur.

			return AllReferenceTypeObjectsIn(assembly)
				.OfType<MemberReference>();
		}

		/// <summary>
		/// Gets all references in the .NET object hierarchy of the specified object.
		/// </summary>
		private static IEnumerable<object> AllReferenceTypeObjectsIn(object obj)
		{
			if (obj == null)
				return Enumerable.Empty<object>();

			var allObjects = new HashSet<object>(new ReferenceEqualityComparer());
			impl(obj, allObjects);
			return allObjects;

			static void impl(object obj, HashSet<object> allObjects)
			{
				if (obj == null || allObjects.Contains(obj))
					return;

				if (!obj.GetType().IsValueType && !obj.GetType().IsPointer && !obj.GetType().IsNullable() && !obj.GetType().IsPrimitive)
				{
					allObjects.Add(obj);
				}

				string assemblyName = obj.GetType().Assembly.GetName().Name;
				if (assemblyName == "System.Private.CoreLib")
					return;


				var allProperties = obj.GetType()
									   .GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
									   .Where(property => property.GetGetMethod() != null)
									   .Where(property => property.GetGetMethod().GetParameters().Length == 0); // exclude indexer

				foreach (var property in allProperties)
				{
					try
					{
						// just trigger getters so that backing fields are initialized.
						property.GetValue(obj, Array.Empty<object>());
					}
					catch
					{
						// amazingly, I have never observed an exception for the sample assemblies,
						// but let's just leave the catch block just in case
					}
				}

				var allFieldValues = obj.GetType()
										.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
										.Where(field => !field.IsStatic || (!obj.GetType().IsValueType && !obj.GetType().IsPointer))
										.Select(field => field.GetValue(obj));

				foreach (var value in allFieldValues)
				{
					impl(value, allObjects);
					if (value is IEnumerable enumerableValue)
						foreach (var element in enumerableValue)
							impl(element, allObjects);
				}
			}
		}

		private static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

		/// <summary>
		/// Locates where the potential compatibility issue would actually be an issue.
		/// </summary>
		private static IEnumerable<MemberReference> DetectIssue(ICompatibilityIssue potentialIssue, IReadOnlyList<MemberReference> references)
		{
			var locations = potentialIssue switch
			{
				IMissingMemberCompatibilityIssue missingMemberIssue => references.Where(reference => reference.RefersTo(missingMemberIssue.MissingMember)),
				MemberAccessibilityReducedIssue issue => references.Where(reference => reference.RefersTo(issue.Member)),
				_ => throw new NotImplementedException()
			};
			return locations;
		}
	}
}
