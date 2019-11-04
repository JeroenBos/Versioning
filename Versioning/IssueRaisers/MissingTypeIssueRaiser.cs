using System;
using System.Collections.Generic;
using System.Linq;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingTypeIssueRaiser : ICompatiblityIssueRaiser<Type>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(Type type, Type? equivalent, IReadOnlyList<Type> candidates)
		{
			if (equivalent == null)
				return new[] { new MissingTypeIssue(type) };
			return Enumerable.Empty<ICompatibilityIssue>();
		}
	}
}
