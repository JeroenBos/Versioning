using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingEventIssueRaiser : ICompatiblityIssueRaiser<EventInfo>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(EventInfo @event, EventInfo? equivalent, IReadOnlyList<EventInfo> candidates)
		{
			if (equivalent == null)
				yield return new MissingEventIssue(@event);
		}
	}

}
