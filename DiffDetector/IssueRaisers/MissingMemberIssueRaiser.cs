using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.DiffDetector.Issues;

namespace Versioning.DiffDetector.IssueRaisers
{
	public class MissingMemberIssueRaiser : ICompatibilityIssueRaiser<IMemberDefinition>
	{
		public static IEnumerable<ICompatibilityIssue> Evaluate(IMemberDefinition member, IMemberDefinition? resolved, IReadOnlyList<IMemberDefinition> candidates)
		{
			if (resolved == null)
			{
				if (!member.GetAccessibility().IsAllowedToChangeTo(AccessAndStaticModifiers.None))
				{
					yield return new MissingMemberIssue(member);
				}
			}
			else if (member is PropertyDefinition property && resolved is PropertyDefinition resolvedProperty)
			{
				if (property.GetMethod != null && resolvedProperty.GetMethod == null
				 && !property.GetMethod.GetAccessibility().IsAllowedToChangeTo(AccessAndStaticModifiers.None))
				{
					yield return new MissingAccessorIssue(property, PropertyAccessor.Get);
				}
				if (property.SetMethod != null && resolvedProperty.SetMethod == null
				 && !property.SetMethod.GetAccessibility().IsAllowedToChangeTo(AccessAndStaticModifiers.None))
				{
					yield return new MissingAccessorIssue(property, PropertyAccessor.Set);
				}
			}
		}

		IEnumerable<ICompatibilityIssue> ICompatibilityIssueRaiser<IMemberDefinition>.Evaluate(IMemberDefinition element, IMemberDefinition? resolved, IReadOnlyList<IMemberDefinition> candidates) => Evaluate(element, resolved, candidates);
	}

}
