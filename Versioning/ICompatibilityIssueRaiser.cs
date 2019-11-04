using System;
using System.Collections.Generic;
using System.Reflection;

namespace Versioning
{
	public interface ICompatiblityIssueRaiser { }
	/// <summary>
	/// Represents a detector for a particular kind of compatibility issue for a particular kind 
	/// of assembly element <typeparamref name="T"/> (class, struct, interface, method, property, etc).
	/// </summary>
	/// <typeparam name="T"> The type of assembly element this can raise issues for.
	/// Maybe the constraint should be relaxed.
	/// For now it should only be one of 
	/// - <see cref="Type"/>
	/// - <see cref="MethodInfo"/>
	/// - <see cref="ConstructorInfo"/>
	/// - <see cref="FieldInfo"/>
	/// - <see cref="PropertyInfo"/>
	/// - <see cref="EventInfo"/>
	/// </typeparam>
	public interface ICompatiblityIssueRaiser<in T> : ICompatiblityIssueRaiser
	{
		/// <summary>
		/// Gets all issues on the specified element and its resolved counterparts in the other assembly.
		/// </summary>
		/// <param name="element"> The assembly element on which to report issues. </param>
		/// <param name="equivalentElements"> The resolved counterparts in the other assembly. Is empty when not found. </param>
		IEnumerable<ICompatibilityIssue> Evaluate(T element, IReadOnlyList<T> equivalentElements);
	}

}
