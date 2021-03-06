﻿using System;
using System.Collections.Generic;
using System.Linq;
using Versioning.DiffDetector.Equality;
using Mono.Cecil;
using Assembly = System.Reflection.Assembly;
using System.Collections.ObjectModel;
using System.IO;
using Versioning.DiffDetector.IssueRaisers;

namespace Versioning.DiffDetector
{
	/// <summary>
	/// So initially the goals are:
	/// - List every public member (class, struct, interface, method, property etc) present in an assembly, A, with major version m
	/// - List every public member from a linked assembly, A, (with major version n<m) used within another assembly, B, by analyzing B
	/// - Check whether there's anything in the second list that's not in the first. 
	///   If there isn't, B is probably compatible with A (v=m), and any assembly depending on B will be okay when it references A (v=m) itself.
	///   
	/// Btw, the strategy I've chosen is to divide this into two problems: 
	/// 1) list all differences between two assembly version (i.e. versions n and m from assembly A)
	/// 2) filter that list of differences based on usage by another assembly, i.e. B.
	/// I'm working on 1) now
	/// </summary>
	public class CompatiblityIssueCollector
	{
		public static CompatiblityIssueCollector Default { get; } = createDefault();
		private static CompatiblityIssueCollector createDefault()
		{
			var issueRaisers = new ICompatibilityIssueRaiser[]
			{
				new MissingMemberIssueRaiser(),
				new MemberAccessibilityReducedIssueRaiser(),
			};
			return new CompatiblityIssueCollector(issueRaisers);
		}

		public IReadOnlyList<ICompatibilityIssueRaiser> IssueRaisers { get; }
		/// <param name="raiseWhenParentIsMissing"> Indicates whether issues should be raised on assembly elements evenwhen the declaring/parent assembly element is missing. </param>
		public CompatiblityIssueCollector(IReadOnlyList<ICompatibilityIssueRaiser> issueRaisers)
		{
			if (issueRaisers == null)
				throw new ArgumentNullException(nameof(issueRaisers));

			this.IssueRaisers = issueRaisers;
		}

		/// <summary>
		/// Returns all compatibility issues between the specified assemblies, as raised by <see cref="IssueRaisers"/>.
		/// </summary>
		public IEnumerable<ICompatibilityIssue> GetCompatibilityIssuesBetween(Assembly assembly, Assembly assemblyHigherVersion)
		{
			if (string.IsNullOrEmpty(assembly.Location))
				throw new ArgumentException("Assembly must have location", nameof(assembly));
			if (string.IsNullOrEmpty(assemblyHigherVersion.Location))
				throw new ArgumentException("Assembly must have location", nameof(assemblyHigherVersion));

			var definition = AssemblyDefinition.ReadAssembly(File.OpenRead(assembly.Location));
			var definitionHigherVersion = AssemblyDefinition.ReadAssembly(File.OpenRead(assemblyHigherVersion.Location));

			return GetCompatibilityIssuesBetween(definition, definitionHigherVersion);
		}

		/// <summary>
		/// Returns all compatibility issues between the specified assemblies, as raised by <see cref="IssueRaisers"/>.
		/// </summary>
		public IEnumerable<ICompatibilityIssue> GetCompatibilityIssuesBetween(AssemblyDefinition assembly, AssemblyDefinition assemblyHigherVersion)
		{
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			if (assemblyHigherVersion == null) throw new ArgumentNullException(nameof(assemblyHigherVersion));

			return assembly.MainModule
						   .Types
						   .Where(type => type.DeclaringType == null) // nested types are handled as members
						   .SelectMany(type => GetIssuesOn(type, this.ResolveType(type, assemblyHigherVersion), Array.Empty<TypeDefinition>()));
		}

		/// <summary>
		/// Traverses the type system and collects all raised issues.
		/// </summary>
		private IEnumerable<ICompatibilityIssue> GetIssuesOn(TypeDefinition type, TypeDefinition? otherType, IReadOnlyList<TypeDefinition> candidates)
		{
			var issues = GetIssuesOn<TypeDefinition>(type, otherType, candidates).ToList();
			if (otherType == null)
				return issues;

			var nestedIssues = type.GetFamilyAndPublicMembers()
				                   .SelectMany(member => member switch
			{
				FieldDefinition f => GetIssuesOn(f, this.ResolveField(f, otherType), this.ResolveFieldCandidates(f, otherType)),
				PropertyDefinition p => GetIssuesOn(p, this.ResolveProperty(p, otherType), this.ResolvePropertyCandidates(p, otherType)),
				MethodDefinition m when m.SemanticsAttributes == MethodSemanticsAttributes.None
								   => GetIssuesOn(m, this.ResolveMethod(m, otherType), this.ResolveMethodCandidates(m, otherType)),
				MethodDefinition _ => Enumerable.Empty<ICompatibilityIssue>(),
				EventDefinition e => GetIssuesOn(e, this.ResolveEvent(e, otherType), this.ResolveEventCandidates(e, otherType)),
				TypeDefinition nt => GetIssuesOn(nt, this.ResolveType(nt, otherType), this.ResolveTypeCandidates(nt, otherType)),
				_ => throw new Exception()
			});
			return issues.Concat(nestedIssues);
		}

		/// <summary>
		/// Tries to find the same type in the specified assembly.
		/// </summary>
		private TypeDefinition? ResolveType(TypeDefinition type, AssemblyDefinition assembly)
		{
			if (type.DeclaringType != null)
			{
				TypeDefinition? declaringTypeInOtherAssembly = ResolveType(type.DeclaringType, assembly);
				if (declaringTypeInOtherAssembly == null)
					return null;
				return ResolveType(type, declaringTypeInOtherAssembly);
			}

			return assembly.MainModule
						   .Types
						   .FirstOrDefault(t => TypeResolutionEqualityComparer.Singleton.Equals(type, t));
		}
		/// <summary>
		/// Tries to find the same (by name and arity) nested type in the specified type.
		/// </summary>
		private TypeDefinition? ResolveType(TypeDefinition type, TypeDefinition containerType)
		{
			return containerType.NestedTypes
								.FirstOrDefault(t => TypeResolutionEqualityComparer.Singleton.Equals(type, t));
		}
		/// <summary>
		/// For simplicity for now there are no type candidates. They must simply match exactly.
		/// </summary>
		private IReadOnlyList<TypeDefinition> ResolveTypeCandidates(TypeDefinition type, TypeDefinition containerType)
		{
			return Array.Empty<TypeDefinition>();
		}

		/// <summary>
		/// Tries to find the exact same method in the specified type.
		/// </summary>
		private MethodDefinition? ResolveMethod(MethodDefinition method, TypeDefinition type)
		{
			return type.Methods
					   .FirstOrDefault(m => MethodResolutionEqualityComparer.Singleton.Equals(m, method));
		}

		/// <summary>
		/// Returns all methods in the specified type with the same name and public, protected and static modifiers.
		/// </summary>
		private IReadOnlyList<MethodDefinition> ResolveMethodCandidates(MethodDefinition method, TypeDefinition type)
		{
			return type.Methods
					   .Where(mi => mi.Name == method.Name)
					   .ToList();
		}

		/// <summary>
		/// Tries to find the exact same event in the specified type.
		/// </summary>
		private EventDefinition? ResolveEvent(EventDefinition @event, TypeDefinition type)
		{
			var candidate = type.Events.FirstOrDefault(e => e.Name == @event.Name);

			if (candidate != null && EventResolutionEqualityComparer.Singleton.Equals(@event, candidate))
				return candidate;
			return null;
		}
		/// <summary>
		/// Returns all events in the specified type with the same name.
		/// </summary>
		private IReadOnlyList<EventDefinition> ResolveEventCandidates(EventDefinition @event, TypeDefinition type)
		{
			return type.Events
				       .Where(e => @event.Name == e.Name)
				       .ToList();
		}

		/// <summary>
		/// Tries to find the exact same property in the specified type.
		/// </summary>
		private PropertyDefinition? ResolveProperty(PropertyDefinition property, TypeDefinition type)
		{
			return type.Properties
					   .Where(p => PropertyResolutionEqualityComparer.Singleton.Equals(p, property))
					   .FirstOrDefault();
		}
		/// <summary>
		/// Returns all properties in the specified type with the same name.
		/// </summary>
		private IReadOnlyList<PropertyDefinition> ResolvePropertyCandidates(PropertyDefinition property, TypeDefinition type)
		{
			return type.Properties
					   .Where(p => property.Name == p.Name)
					   .ToList();
		}

		/// <summary>
		/// Tries to find the same exact field in the specified type.
		/// </summary>
		private FieldDefinition? ResolveField(FieldDefinition field, TypeDefinition type)
		{
			return type.Fields
					   .Where(p => FieldResolutionEqualityComparer.Singleton.Equals(p, field))
					   .FirstOrDefault();
		}
		/// <summary>
		/// Returns all fields in the specified type with the same name.
		/// </summary>
		private IReadOnlyList<FieldDefinition> ResolveFieldCandidates(FieldDefinition field, TypeDefinition type)
		{
			return type.Fields
					   .Where(f => field.Name == f.Name)
					   .ToList();
		}

		/// <summary>
		/// Gets the issues raised by the issue raisers applicable on <paramref name="element"/>, 
		/// given the resolved equivalent of the element in the other assembly, and candidates.
		/// </summary>
		private IEnumerable<ICompatibilityIssue> GetIssuesOn<T>(T element, T? equivalentElement, IReadOnlyList<IMemberDefinition> candidates) where T : class
		{
			return this.GetIssueRaisers<T>()
					   .SelectMany(issueRaiser => issueRaiser.Evaluate(element, equivalentElement, candidates));
		}

		/// <summary>
		/// Gets the issue raisers applicable on <typeparamref name="T"/>.
		/// </summary>
		private IEnumerable<ICompatibilityIssueRaiser<T>> GetIssueRaisers<T>() where T : class
		{
			foreach (var raiser in this.IssueRaisers)
				if (raiser is ICompatibilityIssueRaiser<T> typedRaiser)
					yield return typedRaiser;
		}
	}
}
