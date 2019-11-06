using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

namespace Versioning.Equality
{
	public class PropertyInfoEqualityComparer : IEqualityComparer<PropertyDefinition>
	{
		public static readonly PropertyInfoEqualityComparer Singleton = new PropertyInfoEqualityComparer();

		public bool Equals(PropertyDefinition x, PropertyDefinition y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			return x.FullName == y.FullName;
		}

		public int GetHashCode(PropertyDefinition obj) => throw new NotImplementedException();
	}
}
