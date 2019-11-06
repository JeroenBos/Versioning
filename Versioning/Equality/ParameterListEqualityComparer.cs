using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Versioning.Equality
{
	public class ParameterListEqualityComparer : IEqualityComparer<IReadOnlyList<ParameterInfo>>
	{
		public static readonly ParameterListEqualityComparer Singleton = new ParameterListEqualityComparer();

		public bool Equals(IReadOnlyList<ParameterInfo> x, IReadOnlyList<ParameterInfo> y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if (x == null ^ y == null)
				return false;

			if (x!.Count != y!.Count)
				return false;

			return Enumerable.SequenceEqual(x, y, ParameterEqualityComparer.Singleton);
		}

		public int GetHashCode(IReadOnlyList<ParameterInfo> obj) => throw new NotImplementedException();
	}
}
