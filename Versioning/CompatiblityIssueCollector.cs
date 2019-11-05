using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Versioning.Equality;

namespace Versioning
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
		public IReadOnlyList<ICompatiblityIssueRaiser> IssueRaisers { get; }
		public CompatiblityIssueCollector(IReadOnlyList<ICompatiblityIssueRaiser> issueRaisers)
		{
			if (issueRaisers == null) throw new ArgumentNullException(nameof(issueRaisers));

			this.IssueRaisers = issueRaisers;
		}

		/// <summary>
		/// Returns all compatibility issues between the specified assemblies, as raised by <see cref="IssueRaisers"/>.
		/// </summary>
		public IEnumerable<ICompatibilityIssue> GetCompatibilityIssuesBetween(Assembly assembly, Assembly assemblyHigherVersion)
		{
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			if (assemblyHigherVersion == null) throw new ArgumentNullException(nameof(assemblyHigherVersion));

			return assembly.GetTypes()
						   .Where(type => type.DeclaringType == null) // nested types are handled as members
						   .SelectMany(type => GetIssuesOn(type, this.ResolveType(type, assemblyHigherVersion), Array.Empty<Type>()));
		}

		/// <summary>
		/// Traverses the type system and collects all raised issues.
		/// </summary>
		private IEnumerable<ICompatibilityIssue> GetIssuesOn(Type type, Type? otherType, IReadOnlyList<Type> candidates)
		{
			var issues = GetIssuesOn<Type>(type, otherType, candidates);
			if (otherType == null)
				return issues;

			var nestedIssues = type.GetFamilyAndPublicMembers().SelectMany(memberInfo => memberInfo switch
			{
				FieldInfo fi => GetIssuesOn(fi, this.ResolveField(fi, otherType), this.ResolveFieldCandidates(fi, otherType)),
				PropertyInfo pi => GetIssuesOn(pi, this.ResolveProperty(pi, otherType), this.ResolvePropertyCandidates(pi, otherType)),
				ConstructorInfo ci => GetIssuesOn(ci, this.ResolveConstructor(ci, otherType), this.ResolveConstructorCandidates(ci, otherType)),
				MethodInfo mi => GetIssuesOn(mi, this.ResolveMethod(mi, otherType), this.ResolveMethodCandidates(mi, otherType)),
				EventInfo ei => GetIssuesOn(ei, this.ResolveEvent(ei, otherType), this.ResolveEventCandidates(ei, otherType)),
				Type nt => GetIssuesOn(nt, this.ResolveType(nt, otherType), this.ResolveTypeCandidates(nt, otherType)),
				_ => throw new Exception()
			});
			return issues.Concat(nestedIssues);
		}

		/// <summary>
		/// Tries to find the same type in the specified assembly.
		/// </summary>
		private Type? ResolveType(Type type, Assembly assembly)
		{
			if (type.DeclaringType != null)
			{
				Type? declaringTypeInOtherAssembly = ResolveType(type.DeclaringType, assembly);
				if (declaringTypeInOtherAssembly == null)
					return null;
				return ResolveType(type, declaringTypeInOtherAssembly);
			}

			return assembly.GetTypes()
						   .FirstOrDefault(t => TypeEqualityComparer.Singleton.Equals(type, t));
		}
		/// <summary>
		/// Tries to find the same (by name and arity) nested type in the specified type.
		/// </summary>
		private Type? ResolveType(Type type, Type containerType)
		{
			return containerType.GetNestedTypes(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
								.FirstOrDefault(t => TypeEqualityComparer.Singleton.Equals(type, t));
		}
		/// <summary>
		/// For simplicity for now there are no type candidates. They must simply match exactly.
		/// </summary>
		private IReadOnlyList<Type> ResolveTypeCandidates(Type type, Type containerType)
		{
			return Array.Empty<Type>();
		}

		/// <summary>
		/// Tries to find the exact same method in the specified type.
		/// </summary>
		private MethodInfo? ResolveMethod(MethodInfo method, Type type)
		{
			return type.GetMethodsAccessibleLike(method)
					   .FirstOrDefault(m => MethodInfoEqualityComparer.Singleton.Equals(m, method));
		}

		/// <summary>
		/// Returns all methods in the specified type with the same name and public, protected and static modifiers.
		/// </summary>
		private IReadOnlyList<MethodInfo> ResolveMethodCandidates(MethodInfo method, Type type)
		{
			return type.GetMethodsAccessibleLike(method)
					   .Where(mi => mi.Name == method.Name)
					   .ToList();
		}

		/// <summary>
		/// Tries to find the exact same method in the specified type.
		/// </summary>
		private ConstructorInfo? ResolveConstructor(ConstructorInfo constructor, Type type)
		{
			return type.GetConstructorsAccessibleLike(constructor)
					   .FirstOrDefault(ctor => ConstructorInfoEqualityComparer.Singleton.Equals(ctor, constructor));
		}
		/// <summary>
		/// Returns all methods in the specified type with the same name and public, protected and static modifiers.
		/// </summary>
		private IReadOnlyList<ConstructorInfo> ResolveConstructorCandidates(ConstructorInfo ctor, Type type)
		{
			return type.GetConstructorsAccessibleLike(ctor)
					   .Where(mi => mi.Name == ctor.Name)
					   .ToList();
		}

		/// <summary>
		/// Tries to find the exact same event in the specified type.
		/// </summary>
		private EventInfo? ResolveEvent(EventInfo @event, Type type)
		{
			var candidate = type.GetEvent(@event.Name);

			if (candidate != null && EventInfoEqualityComparer.Singleton.Equals(@event, candidate))
				return candidate;
			return null;
		}
		/// <summary>
		/// Returns all events in the specified type with the same name.
		/// </summary>
		private IReadOnlyList<EventInfo> ResolveEventCandidates(EventInfo @event, Type type)
		{
			return type.GetEvent(@event.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				       .ToSingletonListIfNotNull();
		}

		/// <summary>
		/// Tries to find the exact same property in the specified type.
		/// </summary>
		private PropertyInfo? ResolveProperty(PropertyInfo property, Type type)
		{
			return type.GetPropertiesAccessibleLike(property)
					   .Where(p => PropertyInfoEqualityComparer.Singleton.Equals(p, property))
					   .FirstOrDefault();
		}
		/// <summary>
		/// Returns all properties in the specified type with the same name.
		/// </summary>
		private IReadOnlyList<PropertyInfo> ResolvePropertyCandidates(PropertyInfo property, Type type)
		{
			return type.GetProperty(property.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				       .ToSingletonListIfNotNull();
		}

		/// <summary>
		/// Tries to find the same exact field in the specified type.
		/// </summary>
		private FieldInfo? ResolveField(FieldInfo field, Type type)
		{
			var candidate = type.GetField(field.Name);

			if (candidate != null && FieldInfoEqualityComparer.Singleton.Equals(field, candidate))
				return candidate;
			return null;
		}
		/// <summary>
		/// Returns all fields in the specified type with the same name.
		/// </summary>
		private IReadOnlyList<FieldInfo> ResolveFieldCandidates(FieldInfo field, Type type)
		{
			return type.GetField(field.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				       .ToSingletonListIfNotNull();
		}

		/// <summary>
		/// Gets the issues raised by the issue raisers applicable on <paramref name="element"/>, 
		/// given the resolved equivalent of the element in the other assembly, and candidates.
		/// </summary>
		private IEnumerable<ICompatibilityIssue> GetIssuesOn<T>(T element, T? equivalentElement, IReadOnlyList<T> candidates) where T : class
		{
			return this.GetIssueRaisers<T>()
					   .SelectMany(issueRaiser => issueRaiser.Evaluate(element, equivalentElement, candidates));
		}
		/// <summary>
		/// Gets the issue raisers applicable on <typeparamref name="T"/>.
		/// </summary>
		private IEnumerable<ICompatiblityIssueRaiser<T>> GetIssueRaisers<T>() where T : class
		{
			foreach (var raiser in this.IssueRaisers)
				if (raiser is ICompatiblityIssueRaiser<T> typedRaiser)
					yield return typedRaiser;
		}
	}
}
