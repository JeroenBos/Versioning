using Mono.Cecil;
using System.Collections.Generic;
using Versioning.DiffDetector;

namespace Versioning.UsageDetector
{
	/// <summary>
	/// Represents compatibility issues that have been detected at recorded locations.
	/// </summary>
	public interface IDetectedCompatibilityIssue : ICompatibilityIssue
	{
		/// <summary>
		/// Gets the issue to which the associated locations are relevant.
		/// </summary>
		ICompatibilityIssue Issue { get; }
		/// <summary>
		/// Gets the locations at which the associated issue was detected.
		/// </summary>
		IReadOnlyList<MemberReference> Locations { get; }
	}


	/// <summary>
	/// A default implementation for <see cref="IDetectedCompatibilityIssue"/>.
	/// </summary>
	public class DetectedCompatibilityIssue : IDetectedCompatibilityIssue
	{
		public ICompatibilityIssue Issue { get; }
		public IReadOnlyList<MemberReference> Locations { get; }
		public DetectedCompatibilityIssue(ICompatibilityIssue issue, IReadOnlyList<MemberReference> locations)
			=> (this.Issue, this.Locations) = (issue, locations);
	}
}
