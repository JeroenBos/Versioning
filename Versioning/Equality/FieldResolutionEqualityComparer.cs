using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

namespace Versioning.Equality
{
	/// <summary>
	/// Compares fields for equality in view of (binary) compatibility.
	/// The context is, given two versions of an assembly and a FieldInfo from each, do they represent the same field? 
	/// i.e. if another assembly depends on the older version and at runtime the new version is resolved, are the field stil resolved the same?
	/// The following aspects of a type determine this:
	/// - name
	/// - declaring type
	/// 
	/// I don't even know if the following would be breaking binary compatibility:
	/// - static? abstract? extern? partial? 
	/// </summary>
	class FieldResolutionEqualityComparer : IEqualityComparer<FieldDefinition>
	{
		public static readonly FieldResolutionEqualityComparer Singleton = new FieldResolutionEqualityComparer();
		public bool Equals(FieldDefinition x, FieldDefinition y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			return x.FullName == y.FullName;
		}

		public int GetHashCode(FieldDefinition obj) => throw new NotImplementedException();
	}
}
