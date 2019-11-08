using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingMemberIssueRaiser : ICompatibilityIssueRaiser<IMemberDefinition>
	{
		public static IEnumerable<ICompatibilityIssue> Evaluate(IMemberDefinition member, IMemberDefinition? resolved, IReadOnlyList<IMemberDefinition> candidates)
		{
			if (resolved == null)
			{
				yield return new MissingMemberIssue(member);
			}
			else if (member is PropertyDefinition property && resolved is PropertyDefinition resolvedProperty)
			{
				if (property.GetMethod != null && resolvedProperty.GetMethod == null)
				{
					yield return new MissingAccessorIssue(property, PropertyAccessor.Get);
				}
				if (property.SetMethod != null && resolvedProperty.SetMethod == null)
				{
					yield return new MissingAccessorIssue(property, PropertyAccessor.Set);
				}
			}
		}

		IEnumerable<ICompatibilityIssue> ICompatibilityIssueRaiser<IMemberDefinition>.Evaluate(IMemberDefinition element, IMemberDefinition? resolved, IReadOnlyList<IMemberDefinition> candidates) => Evaluate(element, resolved, candidates);
	}

}
