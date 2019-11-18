using System.Collections.Generic;

namespace Versioning.UsageDetector.Tests
{
	/// <summary>
	/// A simple record type for the type <see cref="(ProcessDelegate? entryPoint, IReadOnlyList{IDetectedCompatibilityIssue} issues)"/>.
	/// </summary>
	public readonly struct EntryPointPlusIssues
	{
/// <summary>
/// Gets a delegate wrapping a call to an assembly entry point in a seperate process.
/// </summary>
		public ProcessDelegate? EntryPoint { get; }
		public IReadOnlyList<IDetectedCompatibilityIssue> Issues { get; }
		public EntryPointPlusIssues(ProcessDelegate? entryPoint, IReadOnlyList<IDetectedCompatibilityIssue> issues)
			=> (EntryPoint, Issues) = (entryPoint, issues);

		public void Deconstruct(out ProcessDelegate? entryPoint, out IReadOnlyList<IDetectedCompatibilityIssue> issues) => (entryPoint, issues) = (EntryPoint, Issues);
		public static implicit operator EntryPointPlusIssues((ProcessDelegate? entryPoint, IReadOnlyList<IDetectedCompatibilityIssue> issues) tuple) => new EntryPointPlusIssues(tuple.entryPoint, tuple.issues);
	}
}
