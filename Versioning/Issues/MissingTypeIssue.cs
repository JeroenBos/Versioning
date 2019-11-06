using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Issues
{
	public class MissingTypeIssue : IMissingMemberCompatibilityIssue
	{
		public Type MissingType { get; }
		public MissingTypeIssue(Type missingType)
		{
			if (missingType == null) throw new ArgumentNullException(nameof(missingType));

			this.MissingType = missingType;
		}

		MemberInfo IMissingMemberCompatibilityIssue.MissingMember => MissingType;
	}
}
