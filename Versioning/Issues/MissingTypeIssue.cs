using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Issues
{
	public class MissingTypeIssue : IMissingMemberCompatibilityIssue
	{
		public TypeDefinition MissingType { get; }
		public MissingTypeIssue(TypeDefinition missingType)
		{
			if (missingType == null) throw new ArgumentNullException(nameof(missingType));

			this.MissingType = missingType;
		}

		IMemberDefinition IMissingMemberCompatibilityIssue.MissingMember => MissingType;
	}
}
