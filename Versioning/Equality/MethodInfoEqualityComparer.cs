using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Equality
{
	class MethodInfoEqualityComparer : IEqualityComparer<MethodInfo>
	{
		public static readonly MethodInfoEqualityComparer Singleton = new MethodInfoEqualityComparer();

		public bool Equals(MethodInfo x, MethodInfo y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			return x.GetParameters().SequenceEqual(y.GetParameters(), ParameterInfoEqualityComparer.Singleton)
				&& x.ReturnType.FullName == y.ReturnType.FullName
				&& x.GetGenericArguments().SequenceEqual(y.GetGenericArguments(), GenericParameterEqualityComparer.Singleton);
		}

		public int GetHashCode(MethodInfo obj) => throw new NotImplementedException();
	}
}
