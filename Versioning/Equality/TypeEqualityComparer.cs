using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Equality
{
	/// <summary>
	/// Compares types by name, arity, namespace, publicness and declaring types.
	/// </summary>
	public class ResolveTypeEqualityComparer : IEqualityComparer<Type>
	{
		public static readonly ResolveTypeEqualityComparer Singleton = new ResolveTypeEqualityComparer();

		public bool Equals(Type? x, Type? y)
		{
			if (x != null && x.IsGenericParameter) throw new ArgumentException("Generic type parameters cannot be resolved", nameof(x));
			if (y != null && y.IsGenericParameter) throw new ArgumentException("Generic type parameters cannot be resolved", nameof(y));

			if (ReferenceEquals(x, y))
				return true;
			if (x == null ^ y == null)
				return false;

			return x!.Name == y!.Name
				&& x.Namespace == y.Namespace
				&& x.GetGenericArguments().Length == y.GetGenericArguments().Length
				&& x.IsPublic == y.IsPublic
				&& Equals(x.DeclaringType, y.DeclaringType);
		}

		public int GetHashCode(Type obj) => throw new NotImplementedException();
	}
}
