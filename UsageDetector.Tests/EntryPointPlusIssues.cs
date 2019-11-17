using System.Collections.Generic;

namespace Versioning.UsageDetector.Tests
{
	public readonly struct EntryPointPlusIssues
	{
		public ProcessDelegate? EntryPoint { get; }
		public IReadOnlyList<IDetectedCompatibilityIssue> Issues { get; }
		public EntryPointPlusIssues(ProcessDelegate? entryPoint, IReadOnlyList<IDetectedCompatibilityIssue> issues)
			=> (EntryPoint, Issues) = (entryPoint, issues);

		public void Deconstruct(out ProcessDelegate? entryPoint, out IReadOnlyList<IDetectedCompatibilityIssue> issues) => (entryPoint, issues) = (EntryPoint, Issues);
		public static implicit operator EntryPointPlusIssues((ProcessDelegate? entryPoint, IReadOnlyList<IDetectedCompatibilityIssue> issues) tuple) => new EntryPointPlusIssues(tuple.entryPoint, tuple.issues);
	}
}
