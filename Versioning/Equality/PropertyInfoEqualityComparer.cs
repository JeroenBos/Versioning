using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Equality
{
	public class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
	{
		public static readonly PropertyInfoEqualityComparer Singleton = new PropertyInfoEqualityComparer();

		public bool Equals(PropertyInfo x, PropertyInfo y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			var xGetter = x.GetGetMethod();
			var yGetter = y.GetGetMethod();
			if (xGetter == null ^ yGetter == null)
				return false;

			var xSetter = x.GetSetMethod();
			var ySetter = y.GetSetMethod();
			if (xSetter == null ^ ySetter == null)
				return false;

			if (xGetter != null && !MethodInfoEqualityComparer.Singleton.Equals(xGetter, yGetter!))
				return false;
			return xSetter != null && MethodInfoEqualityComparer.Singleton.Equals(xSetter, ySetter!);
		}

		public int GetHashCode(PropertyInfo obj) => throw new NotImplementedException();
	}
}
