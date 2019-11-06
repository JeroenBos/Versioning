using System.Collections.Generic;

namespace Versioning.UsageDetector
{
	public class ReferencEqualityComparer : IEqualityComparer<object>
	{
		bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);
		int IEqualityComparer<object>.GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}
}
