using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Versioning
{
	/// <summary>
	/// Just some extension methods that I personally like using.
	/// </summary>
	public static class Extensions
	{
		/// <summary> Creates a sequence out of a single specified element. </summary>
		[DebuggerHidden]
		public static IEnumerable<T> ToSingleton<T>(this T element)
		{
			yield return element;
		}

		/// <summary> Creates a list out of a single specified element if it is not null; returns the empty sequence otherwise. </summary>
		[DebuggerHidden]
		public static IReadOnlyList<T> ToSingletonListIfNotNull<T>(this T element) where T : class
		{
			if (element != null)
				return new[] { element };
			return Array.Empty<T>();
		}

		/// <summary>
		/// Gets the bindingflags for public and static, if present on the specified info.
		/// </summary>
		public static BindingFlags GetAccessModifiers(this MethodBase info)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));

			var result = info.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
			result |= info.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
			return result;
		}

		/// <summary>
		/// Gets all methods on the specified type with the same public, protected and static modifiers as the specified info.
		/// </summary>
		public static IEnumerable<MethodInfo> GetMethodsAccessibleLike(this Type type, MethodBase info)
		{
			var all = type.GetMethods(info.GetAccessModifiers());
			if (info.IsFamily)
				return all.Where(m => m.IsFamily);
			else
				return all;
		}
		/// <summary>
		/// Gets all constructors on the specified type with the same public, protected and static modifiers as the specified info.
		/// </summary>
		public static IEnumerable<ConstructorInfo> GetConstructorsAccessibleLike(this Type type, MethodBase info)
		{
			var all = type.GetConstructors(info.GetAccessModifiers());
			if (info.IsFamily)
				return all.Where(m => m.IsFamily);
			else
				return all;
		}
	}
}
