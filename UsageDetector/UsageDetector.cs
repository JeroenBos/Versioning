﻿using Mono.Cecil;
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
		public static IEnumerable<MemberReference> GetAllMemberReferences(AssemblyDefinition assembly)
		{
			 return AllReferenceTypeObjectsIn(assembly)
			 		.OfType<MemberReference>()
			 		.Where(m => !(m is TypeReference));
		}

		public static IEnumerable<TypeReference> GetAllTypeReferences(AssemblyDefinition assembly)
		{
			// I don't know of a way to list all reference types using Mono.Cecil, so I just retrieve them using reflection. 
			// The alternative is exhaustive list all locations in an assembly where type declarations may exist, but let's not go down that path

			return AllReferenceTypeObjectsIn(assembly)
				.OfType<TypeReference>();
		}


		public static IEnumerable<MemberReference> GetAllTypeAndMemberReferences(AssemblyDefinition assembly)
		{
			return AllReferenceTypeObjectsIn(assembly)
				.OfType<MemberReference>();
		}

		static IEnumerable<object> AllReferenceTypeObjectsIn(object obj)
		{
			if (obj == null)
				return Enumerable.Empty<object>();

			var allObjects = new HashSet<object>(new ReferencEqualityComparer());
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
						// just trigger getters so that backing fields are initialized
						property.GetValue(obj, Array.Empty<object>());
					}
					catch { }
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

		static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

		/// <summary>
		/// Ok, set out to solve the real problem:
		/// Detect all type and member references to a particular assembly.
		/// Detect all differences between two versions of that assembly.
		/// Find the intersection.
		/// </summary>
		public static IEnumerable<DetectedCompatibilityIssue> DetectCompatibilityIssues(
			CompatiblityIssueCollector collector,
			AssemblyDefinition main,
			AssemblyDefinition dependency,
			AssemblyDefinition dependencyHigherVersion)
		{
			var allReferences = GetAllTypeAndMemberReferences(main);
			var usage = allReferences.Where(reference => reference.RefersIn(dependency))
									 .ToList();

			var issues = collector.GetCompatibilityIssuesBetween(dependency, dependencyHigherVersion)
								  .ToList();


			return from issue in issues
				   let locations = DetectIssue(issue, usage).ToList()
				   where locations.Count != 0
				   select new DetectedCompatibilityIssue(issue, locations);
		}

		static bool RefersIn(this MemberReference reference, AssemblyDefinition dependency)
		{
			var type = reference as TypeReference ?? reference.DeclaringType;

			return dependency.FullName == (type.Scope as AssemblyNameReference)?.FullName;
		}

		/// <summary>
		/// Locates where the potential compatibility issue would actually be an issue.
		/// </summary>
		static IEnumerable<MemberReference> DetectIssue(ICompatibilityIssue potentialIssue, IReadOnlyList<MemberReference> references)
		{
			var locations = potentialIssue switch
			{
				IMissingMemberCompatibilityIssue missingMemberIssue => references.Where(missingMemberIssue.MissingMember.Equals),
				MemberAccessibilityReducedIssue issue => references.Where(issue.Member.Equals), // TODO: this could be more lenient, but for now this'll do
				_ => throw new NotImplementedException()
			};
			return locations;
		}
	}
}
