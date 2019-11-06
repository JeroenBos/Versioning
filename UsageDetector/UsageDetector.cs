using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Versioning.UsageDetector
{
	public static class UsageDetector
	{
		public static IEnumerable<MemberReference> GetAllMemberReferences(AssemblyDefinition assembly)
		{
			// it suffices to only check in method bodies, because property/event accessors are implemented as methods as well,
			// and e.g. field initializers are in the constructor. Local functions are also lowered to nonlocal functions, so it's fine
			return from TypeDefinition type in assembly.MainModule.Types
				   from MethodDefinition method in type.Methods
				   where method.HasBody
				   from Instruction instruction in method.Body.Instructions
				   where instruction.Operand is MemberReference reference
				   select instruction.Operand as MemberReference; ; // Hm. The name 'reference' does not exist in the current context
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

				if (!obj.GetType().IsValueType && !obj.GetType().IsPointer)
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

		/// <summary>
		/// Ok, set out to solve the real problem:
		/// Detect all type and member references to a particular assembly.
		/// Detect all differences between two versions of that assembly.
		/// Find the intersection.
		/// </summary>
		public static IEnumerable<DetectedCompatibilityIssue> Main(
			CompatiblityIssueCollector collector,
			AssemblyDefinition main,
			AssemblyDefinition dependency,
			AssemblyDefinition dependencyHigherVersion)
		{
			var usage = GetAllTypeAndMemberReferences(main).Where(reference => reference.RefersIn(dependency))
                                                           .ToList();

			throw new NotImplementedException();
			// var issues = collector.GetCompatibilityIssuesBetween(dependency, dependencyHigherVersion)
			// 					  .ToList();
			// 
			// 
			// return from issue in issues
			// 	   let locations = DetectIssue(issue, usage).ToList()
			// 	   where locations.Count != 0
			// 	   select new DetectedCompatibilityIssue(issue, locations);
		}
		public static bool RefersIn(this MemberReference reference, AssemblyDefinition dependency)
		{
			var type = (reference.Resolve() as TypeDefinition) ?? reference.Resolve().DeclaringType;

			return dependency.Equals(type.Module.Assembly);
		}
		/// <summary>
		/// Locates where the potential compatibility issue would actually be an issue.
		/// </summary>
		static IEnumerable<MemberReference> DetectIssue(ICompatibilityIssue potentialIssue, IReadOnlyList<MemberReference> references)
		{
			var locations = potentialIssue switch
			{
				IMissingMemberCompatibilityIssue missingMemberIssue => references.Where(missingMemberIssue.MissingMember.Equals),
				_ => throw new NotImplementedException()
			};
			return locations;
		}


	}
}
