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
	public class TypeResolutionEqualityComparer : IEqualityComparer<Type>
	{
		public static readonly TypeResolutionEqualityComparer Singleton = new TypeResolutionEqualityComparer();

		public bool Equals(Type? x, Type? y)
		{
			if (x != null && x.IsGenericParameter) throw new ArgumentException("Generic type parameters cannot be compared", nameof(x));
			if (y != null && y.IsGenericParameter) throw new ArgumentException("Generic type parameters cannot be compared", nameof(y));

			if (ReferenceEquals(x, y))
				return true;
			if (x == null ^ y == null)
				return false;

			if (x!.IsInterface != y!.IsInterface)
				return false;
			if (x.IsEnum != y.IsEnum)
				return false;
			if (x.IsClass != y.IsClass)
				return false;
			if (x.IsValueType != y.IsValueType)
				return false;

			return x.Name == y.Name
				&& x.Namespace == y.Namespace
				&& x.GetGenericArguments().Length == y.GetGenericArguments().Length
				&& Equals(x.DeclaringType, y.DeclaringType);
		}

		public int GetHashCode(Type obj) => throw new NotImplementedException();
	}
}
