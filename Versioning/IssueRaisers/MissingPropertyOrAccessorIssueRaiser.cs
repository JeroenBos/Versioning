using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingPropertyOrAccessorIssueRaiser : ICompatiblityIssueRaiser<PropertyInfo>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(PropertyInfo property, PropertyInfo? equivalent, IReadOnlyList<PropertyInfo> candidates)
		{
			if (equivalent != null)
				yield break;

			if (candidates.Count == 0
			 || candidates.Count == 1 && candidates[0].IsStatic() != property.IsStatic())
			{
				yield return new MissingPropertyIssue(property);
			}
			else if (candidates.Count == 1)
			{
				PropertyInfo candidate = candidates[0];

				var propertyAccessibility = property.GetAccessAndStaticModifiers();
				var candidateAccessibility = candidate.GetAccessAndStaticModifiers();
				if (!AccessModifierChangeIsAllowed(propertyAccessibility, candidateAccessibility))
				{
					yield return new MissingPropertyIssue(property);
				}
				else
				{
					var propertyGetter = property.GetGetMethod(true);
					var propertySetter = property.GetSetMethod(true);
					var candidateGetter = candidate.GetGetMethod(true);
					var candidateSetter = candidate.GetSetMethod(true);

					if (propertyGetter != null)
					{
						if (candidateGetter == null)
							yield return new MissingAccessorIssue(property, PropertyAccessor.Get);
						else foreach (var issue in MissingMethodIssueRaiser.EvaluateAccessModifierChange(propertyGetter, candidateGetter))
								yield return new MissingAccessorIssue(property, PropertyAccessor.Get);
					}

					if (propertySetter != null)
					{
						if (candidateSetter == null)
							yield return new MissingAccessorIssue(property, PropertyAccessor.Set);
						else foreach (var issue in MissingMethodIssueRaiser.EvaluateAccessModifierChange(propertySetter, candidateSetter))
								yield return new MissingAccessorIssue(property, PropertyAccessor.Set);
					}

				}
			}
			else
			{
				// maybe hidden by sig properties show up here? TODO
				// indexer overloads TODO
				throw new NotImplementedException();
			}
		}

		private static bool AccessModifierChangeIsAllowed(AccessAndStaticModifiers from, AccessAndStaticModifiers to) => from.AccessModifierChangeIsAllowedTo(to);
	}
}
