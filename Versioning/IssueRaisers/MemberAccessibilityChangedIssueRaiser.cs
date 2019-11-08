using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MemberAccessibilityReducedIssueRaiser : ICompatibilityIssueRaiser<IMemberDefinition>
	{
		/// <param name="parent"> This represents the declaring type, but this extra parameter allows the property to be the 'declaring parent' of an accessor method. </param>
		private static IEnumerable<ICompatibilityIssue> Evaluate(
			IMemberDefinition member,
			IMemberDefinition? resolved,
			IReadOnlyList<IMemberDefinition> candidates,
			IMemberDefinition? declaringTypeOfResolved)
		{
			if (resolved == null)
				yield break;

			if (member is PropertyDefinition property)
			{
				foreach (var accessorIssue in evaluatePropertyAccessors(property, resolved as PropertyDefinition, candidates))
					yield return accessorIssue;
			}

			// if the total (including transitively declaring types, if any) accessibility did not become observably less, do nothing
			var oldAccessibility = member.GetAccessibility();
			var newAccessibility = resolved.GetAccessibility();
			if (oldAccessibility.IsAllowedToChangeTo(newAccessibility))
				yield break;

			// if the new accessibility is not observably less than the new parent accessibility, do nothing: the issue will be raised on the parent
			var newParentAccessibility = declaringTypeOfResolved?.GetAccessibility() ?? AccessAndStaticModifiers.Public;
			if (newParentAccessibility.IsAllowedToChangeTo(newAccessibility))
				yield break;

			yield return new MemberAccessibilityReducedIssue(member, oldAccessibility, newAccessibility);
		}

		private static IEnumerable<ICompatibilityIssue> evaluatePropertyAccessors(PropertyDefinition property, PropertyDefinition? resolved, IReadOnlyList<IMemberDefinition> candidates)
		{
			if (property.GetMethod != null)
			{
				var getterIssues = Evaluate(property.GetMethod, resolved?.GetMethod, candidates.Select(c => (c as PropertyDefinition)?.GetMethod).Where(a => a != null).ToList()!, resolved);
				foreach (var issue in getterIssues)
					yield return new MissingAccessorIssue(property, PropertyAccessor.Get);
			}
			if (property.SetMethod != null)
			{
				var setterIssues = Evaluate(property.SetMethod, resolved?.SetMethod, candidates.Select(c => (c as PropertyDefinition)?.SetMethod).Where(a => a != null).ToList()!, resolved);
				foreach (var issue in setterIssues)
					yield return new MissingAccessorIssue(property, PropertyAccessor.Set);
			}
		}

		IEnumerable<ICompatibilityIssue> ICompatibilityIssueRaiser<IMemberDefinition>.Evaluate(IMemberDefinition element, IMemberDefinition? resolved, IReadOnlyList<IMemberDefinition> candidates) => Evaluate(element, resolved, candidates, resolved?.DeclaringType);
	}
}
