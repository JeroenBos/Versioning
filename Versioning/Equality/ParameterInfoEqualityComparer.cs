using System;
using System.Collections.Generic;
using System.Reflection;

namespace Versioning.Equality
{
	public class ParameterInfoEqualityComparer : IEqualityComparer<ParameterInfo>
	{
		public static readonly ParameterInfoEqualityComparer Singleton = new ParameterInfoEqualityComparer();

		public bool Equals(ParameterInfo x, ParameterInfo y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if (x == null ^ y == null) // maybe just throw if not null?
				return false;

			return x!.Name == y!.Name
				&& x.ParameterType.FullName == y.ParameterType.FullName
				&& x.ParameterType.IsByRef == y.ParameterType.IsByRef
				&& x.IsOut == y.IsOut
				&& x.IsIn == y.IsIn
				&& x.IsOptional == y.IsOptional;
		}

		public int GetHashCode(ParameterInfo obj) => throw new NotImplementedException();
	}
}
