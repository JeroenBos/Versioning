using Mono.Cecil;
using System.Collections.Generic;

namespace Versioning.UsageDetector
{
	public interface IDetectedCompatibilityIssue : ICompatibilityIssue
	{
		ICompatibilityIssue Issue { get; }
		IReadOnlyList<MemberReference> Locations { get; }
	}


	public class DetectedCompatibilityIssue : IDetectedCompatibilityIssue
	{
		public ICompatibilityIssue Issue { get; }
		public IReadOnlyList<MemberReference> Locations { get; }
		public DetectedCompatibilityIssue(ICompatibilityIssue issue, IReadOnlyList<MemberReference> locations)
			=> (this.Issue, this.Locations) = (issue, locations);
	}
}
