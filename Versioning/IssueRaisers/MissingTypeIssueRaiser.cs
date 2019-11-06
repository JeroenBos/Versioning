using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingTypeIssueRaiser : ICompatiblityIssueRaiser<TypeDefinition>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(TypeDefinition type, TypeDefinition? resolved, IReadOnlyList<TypeDefinition> candidates)
		{
			if (resolved == null)
			{
				yield return new MissingTypeIssue(type);
			}
			else
			{
				var accessibility = type.GetAccessAndStaticModifiers();
				var resolvedAccessibility = resolved.GetAccessAndStaticModifiers();
				if (!accessibility.AccessModifierChangeIsAllowedTo(resolvedAccessibility))
				{
					// TODO: return more specified issue?
					yield return new MissingTypeIssue(type);
				}
			}
		}
	}
}
