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

			if (candidates.Count == 0)
			{
				yield return new MissingPropertyOrAccessorIssue(property, property.GetAccessorsEnum());
			}
			else if (candidates.Count == 1)
			{
				PropertyInfo candidate = candidates[0];
				PropertyAccessor missing = property.GetAccessorsEnum().Subtract(candidate.GetAccessorsEnum());

				yield return new MissingPropertyOrAccessorIssue(property, missing);
			}
			else
			{
				// maybe hidden properties show up here? TODO
				throw new NotImplementedException();
			}
		}
	}
}
