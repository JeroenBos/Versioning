using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingEventIssueRaiser : ICompatiblityIssueRaiser<EventDefinition>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(EventDefinition @event, EventDefinition? resolved, IReadOnlyList<EventDefinition> candidates)
		{
			if (resolved == null)
			{
				yield return new MissingEventIssue(@event);
			}
			else
			{
				var accessibility = @event.GetAccessAndStaticModifiers();
				var resolvedAccessibility = resolved.GetAccessAndStaticModifiers();
				if (!accessibility.AccessModifierChangeIsAllowedTo(resolvedAccessibility))
				{
					// TODO: return more specified issue?
					yield return new MissingEventIssue(@event);
				}
			}
		}
	}

}
