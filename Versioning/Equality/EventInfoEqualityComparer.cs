using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Equality
{
	public class EventInfoEqualityComparer : IEqualityComparer<EventInfo>
	{
		public static readonly EventInfoEqualityComparer Singleton = new EventInfoEqualityComparer();
		public bool Equals(EventInfo x, EventInfo y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			if (!TypeResolutionEqualityComparer.Singleton.Equals(x.EventHandlerType, y.EventHandlerType))
				return false;

			if (x.GetAccessAndStaticModifiers() != y.GetAccessAndStaticModifiers())
				return false;
			return true;
		}

		public int GetHashCode(EventInfo obj) => throw new NotImplementedException();
	}
}
