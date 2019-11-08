using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingMethodIssueRaiser : ICompatibilityIssueRaiser<MethodDefinition>
	{
		public static IEnumerable<ICompatibilityIssue> Evaluate(MethodDefinition method, MethodDefinition? resolved, IReadOnlyList<MethodDefinition> candidates)
		{
			if (resolved == null)
			{
				yield return new MissingMethodIssue(method);
			}
			else
			{
				var accessibility = method.GetAccessAndStaticModifiers();
				var resolvedAccessibility = resolved.GetAccessAndStaticModifiers();
				if (!accessibility.AccessModifierChangeIsAllowedTo(resolvedAccessibility))
				{
					// TODO: return more specified issue?
					yield return new MissingMethodIssue(method);
				}
			}
		}

		IEnumerable<ICompatibilityIssue> ICompatibilityIssueRaiser<MethodDefinition>.Evaluate(MethodDefinition method, MethodDefinition? resolved, IReadOnlyList<MethodDefinition> candidates) => Evaluate(method, resolved, candidates);
	}
}
