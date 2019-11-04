using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Versioning.Issues;

namespace Versioning.IssueRaisers
{
	public class MissingFieldIssueRaiser : ICompatiblityIssueRaiser<FieldInfo>
	{
		public IEnumerable<ICompatibilityIssue> Evaluate(FieldInfo field, FieldInfo? equivalent, IReadOnlyList<FieldInfo> candidates)
		{
			if (equivalent == null)
				return new[] { new MissingFieldIssue(field) };
			return Enumerable.Empty<ICompatibilityIssue>();
		}
	}

}
