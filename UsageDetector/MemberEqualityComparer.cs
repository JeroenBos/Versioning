using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Versioning.UsageDetector
{
	/// <summary>
	/// <see cref="Mono.Cecil.IMemberDefinition"/> and <see cref="Mono.Cecil.MemberReference"/> do not implement equality comparison,
	/// so we do that here.
	/// </summary>
	class MemberEqualityComparer : IEqualityComparer<MemberReference>
	{
		public static readonly IEqualityComparer<MemberReference> Instance = new MemberEqualityComparer();

		bool IEqualityComparer<MemberReference>.Equals(MemberReference reference, MemberReference definition)
		{
			if (reference == null) throw new ArgumentNullException(nameof(reference));
			if (definition == null) throw new ArgumentNullException(nameof(definition));

			return impl(reference, definition) || impl(definition, reference);

			static bool impl(MemberReference x, MemberReference y)
			{
				return y switch
				{
					PropertyDefinition property => x.RefersTo(property.GetMethod) || x.RefersTo(property.SetMethod),
					EventDefinition @event => x.RefersTo(@event.AddMethod) || x.RefersTo(@event.RemoveMethod) || x.RefersTo(@event.InvokeMethod),
					_ => x.FullName == y.FullName,
				};
			}
		}
		int IEqualityComparer<MemberReference>.GetHashCode(MemberReference obj) => throw new NotImplementedException();
	}

	static class MemberEqualityComparerExtensions
	{
		public static bool RefersTo(this MemberReference reference, IMemberDefinition definition)
		{
			return definition is MemberReference def && MemberEqualityComparer.Instance.Equals(reference, def);
		}
	}
}
