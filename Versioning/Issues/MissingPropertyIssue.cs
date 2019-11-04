using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Issues
{
	public class MissingEventIssue : ICompatibilityIssue
	{
		public EventInfo Event { get; }
		public MissingEventIssue(EventInfo eventInfo)
		{
			if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));

			this.Event = eventInfo;
		}
	}
}
