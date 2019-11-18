using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

namespace Versioning.DiffDetector.Equality
{
	class EventResolutionEqualityComparer : IEqualityComparer<EventDefinition>
	{
		public static readonly EventResolutionEqualityComparer Singleton = new EventResolutionEqualityComparer();
		public bool Equals(EventDefinition x, EventDefinition y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			// TODO: implement
			return x.FullName == y.FullName;
		}

		public int GetHashCode(EventDefinition obj) => throw new NotImplementedException();
	}
}
