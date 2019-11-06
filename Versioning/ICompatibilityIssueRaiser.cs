using System;
using System.Collections.Generic;
using Mono.Cecil;

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
	/// - <see cref="TypeDefinition"/>
	/// - <see cref="MethodDefinition"/>
	/// - <see cref="FieldDefinition"/>
	/// - <see cref="PropertyDefinition"/>
	/// - <see cref="EventDefinition"/>
	/// </typeparam>
	public interface ICompatiblityIssueRaiser<in T> : ICompatiblityIssueRaiser where T : class
	{
		/// <summary>
		/// Gets all issues on the specified element and its resolved counterparts in the other assembly.
		/// </summary>
		/// <param name="element"> The assembly element on which to report issues. </param>
		/// <param name="resolvedElement"> The resolved counterpart in the other assembly. Is null when not found. 
		/// Note that it not being null doesn't guarantee there is no possible binary compatibility issue; this is just
		/// the element that would be resolved. </param>
		/// <param name="candidates"> The assembly elements in the other assembly that almost matched. </param>
		IEnumerable<ICompatibilityIssue> Evaluate(T element, T? resolvedElement, IReadOnlyList<T> candidates);
	}

}
