using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace Versioning
{
	public class CompatiblityIssueRaiserVisitor
	{
		public IReadOnlyList<ICompatiblityIssueRaiser> IssueRaisers { get; }
		public CompatiblityIssueRaiserVisitor(IReadOnlyList<ICompatiblityIssueRaiser> issueRaisers)
		{
			if (issueRaisers == null) throw new ArgumentNullException(nameof(issueRaisers));

			this.IssueRaisers = issueRaisers;
		}

		/// <summary>
		/// So initially the goals are:
		/// - List every public member (class, struct, interface, method, property etc) present in an assembly, A, with major version m
		/// - List every public member from a linked assembly, A, (with major version n<m) used within another assembly, B, by analyzing B
		/// - Check whether there's anything in the second list that's not in the first. 
		///   If there isn't, B is probably compatible with A (v=m), and any assembly depending on B will be okay when it references A (v=m) itself.
		/// </summary>
		public IEnumerable<ICompatibilityIssue> GetCompatibilityIssuesBetween(Assembly assembly, Assembly sameAssemblyLowerVersion)
		{
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			if (sameAssemblyLowerVersion == null) throw new ArgumentNullException(nameof(sameAssemblyLowerVersion));

			return assembly.GetTypes().SelectMany(type => GetIssuesOn(type, this.ResolveType(type, sameAssemblyLowerVersion)));
		}

		private IEnumerable<ICompatibilityIssue> GetIssuesOn(Type type, Type otherType)
		{
			if (otherType == null)
			{
				return GetIssuesOn(type, Array.Empty<Type>());
			}
			else
			{
				var issues = GetIssuesOn(type, new[] { otherType });
				var nestedIssues = type.GetMembers().SelectMany(memberInfo => memberInfo switch
				{
					FieldInfo fi => GetIssuesOn(fi, this.ResolveField(fi, otherType)),
					PropertyInfo pi => GetIssuesOn(pi, this.ResolveProperty(pi, otherType)),
					ConstructorInfo ci => GetIssuesOn(ci, this.ResolveConstructor(ci, otherType)),
					MethodInfo mi => GetIssuesOn(mi, this.ResolveMethod(mi, otherType)),
					EventInfo ei => GetIssuesOn(ei, this.ResolveEvent(ei, otherType)),
					Type nt => GetIssuesOn(nt, this.ResolveType(nt, otherType)),
					_ => throw new Exception()
				});
				return issues.Concat(nestedIssues);
			}
		}

		/// <summary>
		/// Tries to find the same type in the specified assembly.
		/// </summary>
		private Type ResolveType(Type type, Assembly assembly)
		{
			return assembly.GetType(type.FullName);
		}

		/// <summary>
		/// Tries to find the same (by name and arity) nested type in the specified type.
		/// </summary>
		private Type ResolveType(Type type, Type containerType)
		{
			return containerType.GetMember(type.Name)
								.Where(memberInfo => memberInfo is Type)
								.Select(t => (Type)t)
								.Where(t => equalsByArity(type, t))
								.FirstOrDefault();

			bool equalsByArity(Type x, Type y)
			{
				if (!x.IsConstructedGenericType && !y.IsConstructedGenericType)
					return true;
				if (!y.IsConstructedGenericType)
					return false;
				return x.GetGenericArguments().Length == y.GetGenericArguments().Length;
			}
		}

		/// <summary>
		/// Tries to find the same method in the specified type.
		/// If there is one perfect match, returns that one; otherwise all methods with the same name and public, protected and static modifiers.
		/// </summary>
		private IReadOnlyList<MethodInfo> ResolveMethod(MethodInfo method, Type type)
		{
			var exactMatch = type.GetMethodsAccessibleLike(method)
								 .Where(isExactMatch)
								 .ToList();

			if (exactMatch.Count != 0)
				return exactMatch;

			return type.GetMethodsAccessibleLike(method)
					   .Where(mi => mi.Name == method.Name)
					   .ToList();

			bool isExactMatch(MethodInfo mi)
			{
				return mi.GetParameters().SequenceEqual(method.GetParameters(), ParameterInfoEqualityComparer.Singleton)
					&& mi.ReturnType.FullName == method.ReturnType.FullName
					&& mi.GetGenericArguments().SequenceEqual(method.GetGenericArguments(), GenericParameterEqualityComparer.Singleton);
			}
		}

		/// <summary>
		/// Tries to find the same constructor in the specified type. 
		/// If there is one perfect match, returns that one; otherwise all constructors with the same public, protected and static modifiers.
		/// </summary>
		private IReadOnlyList<ConstructorInfo> ResolveConstructor(ConstructorInfo constructor, Type type)
		{
			var exactMatch = type.GetConstructorsAccessibleLike(constructor)
								 .Where(isExactMatch)
								 .ToList();

			if (exactMatch.Count != 0)
				return exactMatch;

			return type.GetConstructorsAccessibleLike(constructor).ToList();


			bool isExactMatch(ConstructorInfo ctor)
			{
				return ctor.GetParameters().SequenceEqual(constructor.GetParameters(), ParameterInfoEqualityComparer.Singleton);
			}
		}

		/// <summary>
		/// Tries to find the same event in the specified type.
		/// </summary>
		private IReadOnlyList<EventInfo> ResolveEvent(EventInfo @event, Type type)
		{
			var eventInfo = type.GetEvent(@event.Name);
			return eventInfo == null ? Array.Empty<EventInfo>() : new[] { eventInfo };
		}

		/// <summary>
		/// Tries to find the same property in the specified type.
		/// </summary>
		private IReadOnlyList<PropertyInfo> ResolveProperty(PropertyInfo property, Type type)
		{
			var propertyInfo = type.GetProperty(property.Name);
			return propertyInfo == null ? Array.Empty<PropertyInfo>() : new[] { propertyInfo };
		}

		/// <summary>
		/// Tries to find the same field in the specified type.
		/// </summary>
		private IReadOnlyList<FieldInfo> ResolveField(FieldInfo field, Type type)
		{
			var fieldInfo = type.GetField(field.Name);
			return fieldInfo == null ? Array.Empty<FieldInfo>() : new[] { fieldInfo };
		}


		private IEnumerable<ICompatibilityIssue> GetIssuesOn<T>(T element, IReadOnlyList<T> equivalentElements) where T : class
		{
			return this.GetIssueRaisers<T>()
					   .SelectMany(issueRaiser => issueRaiser.Evaluate(element, equivalentElements));
		}

		private IEnumerable<ICompatiblityIssueRaiser<T>> GetIssueRaisers<T>() where T : class
		{
			foreach (var raiser in this.IssueRaisers)
				if (raiser is ICompatiblityIssueRaiser<T> typedRaiser)
					yield return typedRaiser;
		}
	}
}
