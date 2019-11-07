using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

namespace Versioning.Equality
{
	/// <summary>
	/// Compares methods for equality in view of (binary) compatibility.
	/// The context is, given two versions of an assembly and a MethodInfo from each, do they represent the same method? 
	/// i.e. if another assembly depends on the older version and at runtime the new version is resolved, are the methods stil resolved the same?
	/// The following aspects of a type determine this:
	/// - name
	/// - declaring type
	/// - arity
	/// - parameter list and their modifiers out, ref, (in?, params?)
	/// 
	/// I don't even know if the following would be breaking binary compatibility:
	/// - static? abstract? extern? partial? 
	/// - return type?
	/// - return type ref/ref readonly modifiers?
	/// </summary>
	class MethodResolutionEqualityComparer : IEqualityComparer<MethodDefinition>
	{
		public static readonly MethodResolutionEqualityComparer Singleton = new MethodResolutionEqualityComparer();

		public bool Equals(MethodDefinition x, MethodDefinition y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));

			return x.FullName == y.FullName;
		}

		public int GetHashCode(MethodDefinition obj) => throw new NotImplementedException();
	}
}
