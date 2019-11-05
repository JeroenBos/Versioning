using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingFieldIssueRaiser : ICompatiblityIssueRaiser<FieldInfo>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(FieldInfo field, FieldInfo? resolved, IReadOnlyList<FieldInfo> candidates)
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
