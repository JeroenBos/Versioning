using Mono.Cecil;
using System;
using System.Collections.Generic;
using Versioning.UsageDetector;

namespace Versioning.CLI
{
	public static class IssueDescriptions
	{
		public static string ToDisplayString(this IDetectedCompatibilityIssue issue)
		{
			if (issue == null) throw new ArgumentNullException(nameof(issue));

			// this method merely dispatches
			switch (issue.Issue)
			{
				case null: throw new ArgumentException();
				case IMissingMemberCompatibilityIssue m: return m.ToDisplayString(issue.Locations);
				default: return $"An unhandled issue of type '${issue.GetType()}' was detected";
			};
		}
		public static string ToDisplayString(this IMissingMemberCompatibilityIssue issue, IReadOnlyList<MemberReference> locations)
		{
			return $"'{issue.MissingMember}' was not present in the newer dependency, and is referenced {locations.Count} times in the dependent assembly.";
		}


		public static string ToHeaderDisplayString(this ICompatibilityIssue issue)
		{
			return issue switch
			{
				null => throw new ArgumentNullException(nameof(issue)),
				IMissingMemberCompatibilityIssue _ => "The following members were not present in the newer assembly:",
				_ => $"Issues of unhandled type '{issue.GetType()}' were detected: ",
			};
		}
		public static string ToElementDisplayString(this ICompatibilityIssue issue)
		{
			// this method merely dispatches
			switch (issue)
			{
				case null: throw new ArgumentNullException(nameof(issue));
				case IMissingMemberCompatibilityIssue m: return m.ToElementDisplayString();
				default: return $"An unhandled issue of type '{issue.GetType()}' was detected";
			};
		}
		public static string ToElementDisplayString(this IMissingMemberCompatibilityIssue issue)
		{
			if (issue.MissingMember.DeclaringType == null)
			{
				return issue.MissingMember.FullName;
			}
			else
			{
				return issue.MissingMember.DeclaringType.FullName + "::" + issue.MissingMember.Name;
			}
		}
	}
}
