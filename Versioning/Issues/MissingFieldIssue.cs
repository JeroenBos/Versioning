using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Issues
{
	public class MissingFieldIssue : IMissingMemberCompatibilityIssue
	{
		public FieldDefinition MissingField { get; }
		public MissingFieldIssue(FieldDefinition missingField)
		{
			if (missingField == null) throw new ArgumentNullException(nameof(missingField));

			this.MissingField = missingField;
		}

		IMemberDefinition IMissingMemberCompatibilityIssue.MissingMember => MissingField;
	}
}
