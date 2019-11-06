using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingPropertyOrAccessorIssueRaiser : ICompatiblityIssueRaiser<PropertyDefinition>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(PropertyDefinition property, PropertyDefinition? resolved, IReadOnlyList<PropertyDefinition> candidates)
		{
			if (resolved == null)
			{
				yield return new MissingPropertyIssue(property);
			}
			else
			{
				var accessibility = property.GetAccessAndStaticModifiers();
				var resolvedAccessibility = resolved.GetAccessAndStaticModifiers();
				if (!accessibility.AccessModifierChangeIsAllowedTo(resolvedAccessibility))
				{
					yield return new MissingPropertyIssue(property);
				}
				else
				{
					if (property.GetMethod != null)
					{
						var getterIssues = MissingMethodIssueRaiser.Evaluate(property.GetMethod, resolved?.GetMethod, candidates.Select(c => c.GetMethod).Where(a => a != null).ToList());
						foreach (var issue in getterIssues)
							yield return new MissingAccessorIssue(property, PropertyAccessor.Get);
					}
					if (property.SetMethod != null)
					{
						var setterIssues = MissingMethodIssueRaiser.Evaluate(property.SetMethod, resolved?.SetMethod, candidates.Select(c => c.SetMethod).Where(a => a != null).ToList());
						foreach (var issue in setterIssues)
							yield return new MissingAccessorIssue(property, PropertyAccessor.Set);
					}
				}
			}
		}
	}
}
