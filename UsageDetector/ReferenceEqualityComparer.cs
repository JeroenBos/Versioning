using System.Collections.Generic;

namespace Versioning.UsageDetector
{
	/// <summary>
	/// Compares objects for equality by reference.
	/// </summary>
	class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);
		int IEqualityComparer<object>.GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}
}
