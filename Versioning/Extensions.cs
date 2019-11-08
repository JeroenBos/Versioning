using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Versioning
{
	public static class Extensions
	{

		/// <summary> Concatenates all specified sequences. </summary>
		public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sources)
		{
			return sources.Concat();
		}
		/// <summary> Concatenates all specified sequences. </summary>
		public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> sources)
		{
			foreach (var sequence in sources)
				foreach (T element in sequence)
					yield return element;
		}

		public static IEnumerable<T> Unfold<T>(this T element, Func<T, T> selectNext) where T : class
		{
			for (var result = element; result != null; result = selectNext(result))
			{
				yield return result;
			}
		}
		public static IEnumerable<IMemberDefinition> GetDeclaringTypesAndSelf(this IMemberDefinition member)
		{
			return member.Unfold(member => member.DeclaringType);
		}
		/// <summary>
		/// Returns a value representing the accessibility modifiers of the specified member.
		/// </summary>
		public static AccessAndStaticModifiers GetAccessibilityModifiers(this IMemberDefinition member) => member.GetAccessAndStaticModifiers() & AccessAndStaticModifiers.AccessMask;
		public static AccessAndStaticModifiers GetAccessibility(this IMemberDefinition member) => member.GetAccessibilityAndStatic() & AccessAndStaticModifiers.AccessMask;

		public static AccessAndStaticModifiers GetAccessibilityAndStatic(this IMemberDefinition member)
		{
			return member.GetDeclaringTypesAndSelf()
				         .Select(GetAccessAndStaticModifiers)
				         .Aggregate(GetLeastAccessible);
		}


		public static AccessAndStaticModifiers GetAccessibilityAndStatic(this IMemberDefinition member, out AccessAndStaticModifiers directAccessibility)
		{
			directAccessibility = member.GetAccessAndStaticModifiers();
			return member.GetAccessibilityAndStatic();
		}


		/// <summary>
		/// Returns a value representing the access modifiers and the presense of the static modifier of the specified member.
		/// </summary>
		public static AccessAndStaticModifiers GetAccessAndStaticModifiers(this IMemberDefinition member)
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			AccessAndStaticModifiers accessFlags = member switch
			{
				MethodDefinition method => (AccessAndStaticModifiers)method.Attributes,
				PropertyDefinition property => property.GetMethod?.GetAccessAndStaticModifiers() ?? 0 | property.SetMethod?.GetAccessAndStaticModifiers() ?? 0,
				EventDefinition @event => @event.AddMethod?.GetAccessAndStaticModifiers() ?? 0 | @event.RemoveMethod?.GetAccessAndStaticModifiers() ?? 0,
				FieldDefinition field => (AccessAndStaticModifiers)field.Attributes,
				TypeDefinition type => (type.Attributes & TypeAttributes.VisibilityMask) switch
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
		public static bool IsStatic(this TypeDefinition type) => type.IsAbstract && type.IsSealed;

		/// <summary>
		/// Gets whether the property has the access modifier 'protected'.
		/// </summary>
		public static bool IsFamily(this PropertyDefinition property)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));

			return (property.GetMethod?.IsFamily ?? false) || (property.SetMethod?.IsFamily ?? false);
		}

		/// <summary>
		/// Gets whether the property has the access modifier 'static'.
		/// </summary>
		public static bool IsStatic(this PropertyDefinition property)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));

			return (property.GetMethod?.IsStatic ?? false) || (property.SetMethod?.IsStatic ?? false);
		}

		/// <summary>
		/// Gets whether the property has the access modifier 'public'.
		/// </summary>
		public static bool IsPublic(this PropertyDefinition property)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));

			return (property.GetMethod?.IsPublic ?? false) || (property.SetMethod?.IsPublic ?? false);
		}

		/// <summary>
		/// Returns whether the access modifier is allowed to change from <paramref name="from"/> to <paramref name="to"/>
		/// without being observable outside of the assembly.
		/// </summary>
		public static bool IsAllowedToChangeTo(this AccessAndStaticModifiers from, AccessAndStaticModifiers to)
		{
			if ((from & AccessAndStaticModifiers.Static) != (to & AccessAndStaticModifiers.Static))
				return false;

			return from <= to || from <= AccessAndStaticModifiers.Assembly;
		}

		/// <summary>
		/// Gets the members on the specified type that are visible outside of the assemby.
		/// </summary>
		public static IReadOnlyList<IMemberDefinition> GetFamilyAndPublicMembers(this TypeDefinition type)
		{
			return Concat<IMemberDefinition>(type.Methods, type.Fields, type.Properties, type.Events, type.NestedTypes)
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

		public static AccessAndStaticModifiers GetLeastAccessible(AccessAndStaticModifiers a, AccessAndStaticModifiers b)
		{
			if ((a & AccessAndStaticModifiers.Static) != (b & AccessAndStaticModifiers.Static))
				return default;

			return min(a & AccessAndStaticModifiers.AccessMask, b & AccessAndStaticModifiers.AccessMask);

			static AccessAndStaticModifiers min(AccessAndStaticModifiers x, AccessAndStaticModifiers y)
				=> (AccessAndStaticModifiers)Math.Min((int)x, (int)y);
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
