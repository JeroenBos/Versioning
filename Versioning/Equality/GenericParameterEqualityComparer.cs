using System;
using System.Collections.Generic;

namespace Versioning.Equality
{
	public class GenericParameterEqualityComparer : IEqualityComparer<Type>
	{
		public static readonly GenericParameterEqualityComparer Singleton = new GenericParameterEqualityComparer();

		public bool Equals(Type x, Type y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));
			if (!x.IsGenericParameter) throw new ArgumentException("The specified type must be a generic type parameter", nameof(x));
			if (!y.IsGenericParameter) throw new ArgumentException("The specified type must be a generic type parameter", nameof(y));

			return x.FullName == y.FullName
				&& x.GenericParameterAttributes == y.GenericParameterAttributes
				&& x.GenericParameterPosition == y.GenericParameterPosition;
			// TODO: constraints ?
		}

		public int GetHashCode(Type obj) => throw new NotImplementedException();
	}
}
