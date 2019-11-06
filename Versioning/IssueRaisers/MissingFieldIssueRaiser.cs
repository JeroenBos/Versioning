using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingFieldIssueRaiser : ICompatiblityIssueRaiser<FieldDefinition>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(FieldDefinition field, FieldDefinition? resolved, IReadOnlyList<FieldDefinition> candidates)
		{
			if (resolved == null)
			{
				yield return new MissingFieldIssue(field);
			}
			else
			{
				var accessibility = field.GetAccessAndStaticModifiers();
				var resolvedAccessibility = resolved.GetAccessAndStaticModifiers();
				if (!accessibility.AccessModifierChangeIsAllowedTo(resolvedAccessibility))
				{
					// TODO: return more specified issue?
					yield return new MissingFieldIssue(field);
				}
			}
		}
	}

}
