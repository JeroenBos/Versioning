using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

namespace Versioning.Issues
{

	public interface IMissingMemberCompatibilityIssue : ICompatibilityIssue
	{
		IMemberDefinition MissingMember { get; }
	}
	public class MissingMemberIssue : IMissingMemberCompatibilityIssue
	{
		public IMemberDefinition MissingMember { get; }
		public MissingMemberIssue(IMemberDefinition member)
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			this.MissingMember = member;
		}
	}
}
