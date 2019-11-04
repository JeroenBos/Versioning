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
		/// Gets the bindingflags for public/nonpublic and static/instance of the specified member info.
		/// </summary>
		public static BindingFlags GetAccessibilityBindingFlags(this MemberInfo info)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));

			var (isStatic, isPublic) = info switch
			{
				MethodBase method => (method.IsStatic, method.IsPublic),
				PropertyInfo property => (property.IsStatic(), property.IsFamily()),
				_ => throw new NotImplementedException()
			};

			return (isStatic ? BindingFlags.Static : BindingFlags.Instance)
				 | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic);
		}

		/// <summary>
		/// Gets all methods on the specified type with the same public, protected and static modifiers as the specified info.
		/// </summary>
		public static IEnumerable<MethodInfo> GetMethodsAccessibleLike(this Type type, MethodBase info)
		{
			var all = type.GetMethods(info.GetAccessibilityBindingFlags());
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
			var all = type.GetConstructors(info.GetAccessibilityBindingFlags());
			if (info.IsFamily)
				return all.Where(m => m.IsFamily);
			else
				return all;
		}

		/// <summary>
		/// Gets all constructors on the specified type with the same public, protected and static modifiers as the specified info.
		/// </summary>
		public static IEnumerable<PropertyInfo> GetPropertiesAccessibleLike(this Type type, PropertyInfo info)
		{
			var all = type.GetProperties(info.GetAccessibilityBindingFlags());
			if (info.IsFamily())
				return all.Where(IsFamily);
			else
				return all;
		}

		/// <summary>
		/// Gets whether the property has the access modifier 'protected'.
		/// </summary>
		public static bool IsFamily(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

			return !propertyInfo.IsPublic()
				&& propertyInfo.GetAccessors(true).Any(accessor => accessor.IsFamily);
		}

		/// <summary>
		/// Gets whether the property has the access modifier 'static'.
		/// </summary>
		public static bool IsStatic(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

			return propertyInfo.GetAccessors(true).Any(accessor => accessor.IsStatic);
		}

		/// <summary>
		/// Gets whether the property has the access modifier 'public'.
		/// </summary>
		public static bool IsPublic(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

			return propertyInfo.GetAccessors(true).Any(accessor => accessor.IsPublic);
		}
	}
}
