using System;
using System.Collections.Generic;
using System.Linq;

namespace Versioning.IssueRaisers
{
	public class MissingTypeIssueRaiser : ICompatiblityIssueRaiser<Type>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(Type element, Type? equivalent, IReadOnlyList<Type> equivalentElements)
		{
			if (equivalent == null)
				return new[] { new MissingTypeIssue(element) };
			return Enumerable.Empty<ICompatibilityIssue>();
		}
	}
}
