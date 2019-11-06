using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Versioning.Issues
{
	public class MissingTypeIssue : ICompatibilityIssue
	{
		public TypeDefinition MissingType { get; }
		public MissingTypeIssue(TypeDefinition missingType)
		{
			if (missingType == null) throw new ArgumentNullException(nameof(missingType));

			this.MissingType = missingType;
		}
	}
}
