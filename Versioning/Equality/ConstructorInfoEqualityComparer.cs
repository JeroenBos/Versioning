using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Equality
{
	class ConstructorInfoEqualityComparer : IEqualityComparer<ConstructorInfo>
	{
		public static readonly ConstructorInfoEqualityComparer Singleton = new ConstructorInfoEqualityComparer();

		public bool Equals(ConstructorInfo x, ConstructorInfo y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));


			return x.GetParameters().SequenceEqual(y.GetParameters(), ParameterInfoEqualityComparer.Singleton);
		}

		public int GetHashCode(ConstructorInfo obj) => throw new NotImplementedException();
	}
}
