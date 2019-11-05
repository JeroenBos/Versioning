using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Equality
{
	public class MethodInfoEqualityComparer : IEqualityComparer<MethodInfo>
	{
		public static readonly MethodInfoEqualityComparer Singleton = new MethodInfoEqualityComparer();

		public bool Equals(MethodInfo x, MethodInfo y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			if (x.Name != y.Name)
				return false;

			if (x.GetAccessAndStaticModifiers() != y.GetAccessAndStaticModifiers())
				return false;

			if (!x.GetParameters().SequenceEqual(y.GetParameters(), ParameterInfoEqualityComparer.Singleton))
				return false;
			if (x.ReturnType.FullName != y.ReturnType.FullName)
				return false;
			if (!x.GetGenericArguments().SequenceEqual(y.GetGenericArguments(), GenericParameterEqualityComparer.Singleton))
				return false;
			return true;
		}

		public int GetHashCode(MethodInfo obj) => throw new NotImplementedException();
	}
}
