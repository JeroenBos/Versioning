using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Versioning
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

			return Enumerable.SequenceEqual(x, y, ParameterInfoEqualityComparer.Singleton);
		}

		public int GetHashCode(IReadOnlyList<ParameterInfo> obj) => throw new NotImplementedException();
	}


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

	public class GenericParameterEqualityComparer : IEqualityComparer<Type>
	{
		public static readonly GenericParameterEqualityComparer Singleton = new GenericParameterEqualityComparer();

		public bool Equals(Type x, Type y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));
			if (!x.IsGenericParameter) throw new ArgumentException("The specified type must be a generic type parameter", nameof(x));
			if (!y.IsGenericParameter) throw new ArgumentException("The specified type must be a generic type parameter", nameof(y));

			return x.FullName == y.FullName
				&& x.GenericParameterAttributes == y.GenericParameterAttributes
				&& x.GenericParameterPosition == y.GenericParameterPosition;
			// TODO: constraints ?
		}

		public int GetHashCode(Type obj) => throw new NotImplementedException();
	}
}
