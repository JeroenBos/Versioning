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
		public static BindingFlags GetAccessibilityBindingFlags(this MemberInfo member) => member.GetAccessAndStaticModifiers().ToBindingFlags();


		/// <summary>
		/// Returns a value representing the accessibility modifiers of the specified member.
		/// </summary>
		public static AccessAndStaticModifiers GetAccessibilityModifiers(this MemberInfo member) => member.GetAccessAndStaticModifiers() & AccessAndStaticModifiers.AccessMask;

		/// <summary>
		/// Converts the specified accessibility level and static flag to public/nonpublic and static/instance binding flags.
		/// </summary>
		public static BindingFlags ToBindingFlags(this AccessAndStaticModifiers level)
		{
			if ((level & ~AccessAndStaticModifiers.Mask) != 0) throw new ArgumentException(nameof(level));

			bool isStatic = (level & AccessAndStaticModifiers.Static) != 0;
			bool isPublic = (level & AccessAndStaticModifiers.AccessMask) == AccessAndStaticModifiers.Public;

			return (isStatic ? BindingFlags.Static : BindingFlags.Instance)
				 | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic);
		}

		/// <summary>
		/// Returns a value representing the access modifiers and the presense of the static modifier of the specified member.
		/// </summary>
		public static AccessAndStaticModifiers GetAccessAndStaticModifiers(this MemberInfo member)
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			AccessAndStaticModifiers accessFlags = member switch
			{
				MethodBase method => (AccessAndStaticModifiers)method.Attributes,
				PropertyInfo property => property.GetGetMethod(true)?.GetAccessAndStaticModifiers() ?? 0 | property.GetSetMethod(true)?.GetAccessAndStaticModifiers() ?? 0,
				EventInfo @event => @event.GetAddMethod(true)?.GetAccessAndStaticModifiers() ?? 0 | @event.GetRemoveMethod(true)?.GetAccessAndStaticModifiers() ?? 0,
				FieldInfo field => (AccessAndStaticModifiers)field.Attributes,
				Type type => (type.Attributes & TypeAttributes.VisibilityMask) switch
				{
					TypeAttributes.NotPublic => AccessAndStaticModifiers.Assembly,
					TypeAttributes.Public => AccessAndStaticModifiers.Public,
					TypeAttributes.NestedPublic => AccessAndStaticModifiers.Public,
					TypeAttributes.NestedPrivate => AccessAndStaticModifiers.Private,
					TypeAttributes.NestedFamily => AccessAndStaticModifiers.Family,
					TypeAttributes.NestedAssembly => AccessAndStaticModifiers.Assembly,
					TypeAttributes.NestedFamANDAssem => AccessAndStaticModifiers.FamANDAssem,
					TypeAttributes.NestedFamORAssem => AccessAndStaticModifiers.FamORAssem,
					_ => throw new Exception(),
				} | (type.IsStatic() ? AccessAndStaticModifiers.Static : 0),
				_ => throw new InvalidOperationException()
			};
			return accessFlags & AccessAndStaticModifiers.Mask;
		}

		public static bool IsStatic(this Type type) => type.IsAbstract && type.IsSealed;

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

		public static bool AccessModifierChangeIsAllowedTo(this AccessAndStaticModifiers from, AccessAndStaticModifiers to)
		{
			if ((from & AccessAndStaticModifiers.Static) != (to & AccessAndStaticModifiers.Static))
				return false;

			// TODO: check protected -> private protected is not allowed
			bool wasFamily = (from & AccessAndStaticModifiers.Family) == AccessAndStaticModifiers.Family;
			bool isFamily = (to & AccessAndStaticModifiers.Family) == AccessAndStaticModifiers.Family;

			bool wasPublic = (from & AccessAndStaticModifiers.Public) == AccessAndStaticModifiers.Public;
			bool isPublic = (to & AccessAndStaticModifiers.Public) == AccessAndStaticModifiers.Public;


			if (wasPublic && !isPublic)
				return false;

			// note that '!isPublic' is redundant: per the but layout, if xFamily is false, then xPublic must be false as well
			if (wasFamily && !isFamily && !isPublic)
				return false;

			// every other access modifier change does not affect compatibility
			return true;
		}

		/// <summary>
		/// Gets the members on the specified type that are visible outside of the assemby.
		/// </summary>
		public static IReadOnlyList<MemberInfo> GetFamilyAndPublicMembers(this Type type)
		{
			return type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
					   .Where(member => member.GetAccessibilityModifiers().IsFamilyOrPublic())
					   .ToList();
		}

		/// <summary>
		/// Gets whether the specified accessibility is visible outside of the assembly.
		/// </summary>
		public static bool IsFamilyOrPublic(this AccessAndStaticModifiers accessAndStaticModifiers)
		{
			switch (accessAndStaticModifiers & AccessAndStaticModifiers.AccessMask)
			{
				case AccessAndStaticModifiers.Private:
				case AccessAndStaticModifiers.FamANDAssem:
				case AccessAndStaticModifiers.Assembly:
					return false;
				case AccessAndStaticModifiers.Family:
				case AccessAndStaticModifiers.FamORAssem:
				case AccessAndStaticModifiers.Public:
					return true;
				case AccessAndStaticModifiers.Static:
				case AccessAndStaticModifiers.None:
				default:
					throw new ArgumentException(nameof(accessAndStaticModifiers));
			}
		}
	}

	public enum AccessAndStaticModifiers
	{
		/// <summary>
		/// Member access mask - Use this mask to retrieve accessibility information.
		/// </summary>
		AccessMask = 0x0007,
		/// <summary>
		/// Member access and static mask.
		/// </summary>
		Mask = AccessMask | Static,

		None = 0,
		/// <summary>
		/// Accessible only by the parent type. 
		/// </summary>
		Private = 0x0001,
		/// <summary>
		/// Accessible by sub-types only in this Assembly.
		/// </summary>
		FamANDAssem = 0x0002,
		/// <summary>
		/// Accessibly by anyone in the Assembly.
		/// </summary>
		Assembly = 0x0003,
		/// <summary>
		/// Accessible only by type and sub-types.
		/// </summary>
		Family = 0x0004,
		/// <summary>
		/// Accessibly by sub-types anywhere, plus anyone in assembly.
		/// </summary>
		FamORAssem = 0x0005,
		/// <summary>
		/// Accessibly by anyone who has visibility to this scope.    
		/// </summary>
		Public = 0x0006,
		Static = 0x0010,
	}
}
