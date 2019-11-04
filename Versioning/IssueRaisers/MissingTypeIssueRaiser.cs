using System;
using System.Collections.Generic;
using System.Linq;

namespace Versioning.IssueRaisers
{
	public class MissingTypeIssueRaiser : ICompatiblityIssueRaiser<Type>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(Type element, IReadOnlyList<Type> equivalentElements)
		{
			if (equivalentElements.Count == 0)
				return new[] { new MissingTypeIssue(element) };
			return Enumerable.Empty<ICompatibilityIssue>();
		}
	}
}
