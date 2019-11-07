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
			if(issue == null) throw new ArgumentNullException(nameof(issue));

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



		public static string ToDisplayString(this ICompatibilityIssue issue)
		{
			// this method merely dispatches
			switch (issue)
			{
				case null: throw new ArgumentNullException(nameof(issue));
				case IMissingMemberCompatibilityIssue m: return m.ToDisplayString();
				default: return $"An unhandled issue of type '${issue.GetType()}' was detected";
			};
		}
		public static string ToDisplayString(this IMissingMemberCompatibilityIssue issue)
		{
			return $"'{issue.MissingMember}' was not present in the newer dependency.";
		}
	}
}
