using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Versioning.Equality
{
	/// <summary>
	/// Compares types for equality in view of (binary) compatibility.
	/// The context is, given two versions of an assembly, are the types the same type? 
	/// i.e. if another assembly depends on the older version and at runtime the new version is resolved, are the types stil resolved the same?
	/// The following aspects of a type determine this:
	/// - name
	/// - declaring namespace/type
	/// - arity
	/// - kind (interface, enum, etc)
	/// 
	/// </summary>
	public class TypeResolutionEqualityComparer : IEqualityComparer<TypeDefinition>
	{
		public static readonly TypeResolutionEqualityComparer Singleton = new TypeResolutionEqualityComparer();

		public bool Equals(TypeDefinition? x, TypeDefinition? y)
		{
			if (x != null && x.IsGenericParameter) throw new ArgumentException("Generic type parameters cannot be compared", nameof(x));
			if (y != null && y.IsGenericParameter) throw new ArgumentException("Generic type parameters cannot be compared", nameof(y));

			if (ReferenceEquals(x, y))
				return true;
			if (x == null ^ y == null)
				return false;

			return x!.IsEnum == y!.IsEnum 
				&& x.IsInterface == y.IsInterface
				&& x.IsClass == y.IsClass
				&& y.IsValueType == y.IsValueType
				&& x.FullName == y.FullName;
		}

		public int GetHashCode(TypeDefinition obj) => throw new NotImplementedException();
	}
}
