using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingMethodIssueRaiser : ICompatiblityIssueRaiser<MethodInfo>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(MethodInfo method, MethodInfo? resolved, IReadOnlyList<MethodInfo> candidates)
		{
			if (resolved != null)
				yield break;

			if (candidates.Count == 0
			 || candidates.Count == 1 && candidates[0].IsStatic != method.IsStatic)
			{
				yield return new MissingMethodIssue(method);
			}
			else
			{
				yield return new MissingMethodIssue(method);
				// we could further delve into why the candidate was rejected here and yield more specific issues
				// but let's not do that for now
				// foreach (var issue in EvaluateAccessModifierChange(method, candidates[0]))
				//    yield return issue;
			}
		}

		internal static IEnumerable<MissingMethodIssue> EvaluateAccessModifierChange(MethodInfo method, MethodInfo candidate)
		{
			var methodAccessibility = method.GetAccessAndStaticModifiers();
			var candidateAccessibility = candidate.GetAccessAndStaticModifiers();
			if (!AccessModifierChangeIsAllowed(methodAccessibility, candidateAccessibility))
			{
				yield return new MissingMethodIssue(method);
			}
		}


		private static bool AccessModifierChangeIsAllowed(AccessAndStaticModifiers from, AccessAndStaticModifiers to) => from.AccessModifierChangeIsAllowedTo(to);
	}
}
