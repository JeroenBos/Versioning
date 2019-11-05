using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Equality
{
	public class FieldInfoEqualityComparer : IEqualityComparer<FieldInfo>
	{
		public static readonly FieldInfoEqualityComparer Singleton = new FieldInfoEqualityComparer();
		public bool Equals(FieldInfo x, FieldInfo y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			if (!TypeResolutionEqualityComparer.Singleton.Equals(x.FieldType, y.FieldType))
				return false;

			if (x.GetAccessAndStaticModifiers() != y.GetAccessAndStaticModifiers())
				return false;
			return true;
		}

		public int GetHashCode(FieldInfo obj) => throw new NotImplementedException();
	}
}
