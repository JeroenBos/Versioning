using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Versioning.Issues
{
	public class MissingEventIssue : IMissingMemberCompatibilityIssue
	{
		public EventInfo MissingEvent { get; }
		public MissingEventIssue(EventInfo eventInfo)
		{
			if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));

			this.MissingEvent = eventInfo;
		}

		MemberInfo IMissingMemberCompatibilityIssue.MissingMember => MissingEvent;
	}
}
