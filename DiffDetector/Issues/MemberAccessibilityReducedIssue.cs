using System;
using Mono.Cecil;

namespace Versioning.DiffDetector.Issues
{
	public class MemberAccessibilityReducedIssue : ICompatibilityIssue
	{
		public IMemberDefinition Member { get; }
		public AccessAndStaticModifiers From { get; }
		public AccessAndStaticModifiers To { get; }

		public MemberAccessibilityReducedIssue(IMemberDefinition member, AccessAndStaticModifiers from, AccessAndStaticModifiers to)
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			this.Member = member;
			this.From = from;
			this.To = to;
		}
	}
}
