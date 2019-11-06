using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Issues
{
	public class MissingPropertyIssue : IMissingMemberCompatibilityIssue
	{
		public PropertyInfo MissingProperty { get; }
		public MissingPropertyIssue(PropertyInfo property)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));

			this.MissingProperty = property;
		}

		MemberInfo IMissingMemberCompatibilityIssue.MissingMember => MissingProperty;
	}


	public class MissingAccessorIssue : MissingMethodIssue
	{
		public PropertyAccessor Accessor { get; }
		public PropertyInfo Property { get; }
		public MissingAccessorIssue(PropertyInfo property, PropertyAccessor missingAccessor)
			: base(property.Select(missingAccessor) ?? throw new ArgumentException("The accessor does not exist"))
		{
			this.Property = property;
			this.Accessor = missingAccessor;
		}

	}

	[Flags]
	public enum PropertyAccessor
	{
		None = 0,
		Get = 1,
		Set = 2,
		Both = Get + Set,
	}
	public static class PropertyAccessorsExtensions
	{
		public static PropertyAccessor GetAccessorsEnum(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

			return (propertyInfo.CanRead ? PropertyAccessor.Get : PropertyAccessor.None)
				| (propertyInfo.CanWrite ? PropertyAccessor.Set : PropertyAccessor.None);
		}
		public static PropertyAccessor Subtract(this PropertyAccessor _accessor, PropertyAccessor accessor)
		{
			return (int)_accessor - accessor;
		}
		public static MethodInfo? Select(this PropertyInfo property, PropertyAccessor accessor)
		{
			if (accessor != PropertyAccessor.Get && accessor != PropertyAccessor.Set)
				throw new ArgumentException(nameof(accessor));

			if (accessor == PropertyAccessor.Get)
				return property.GetMethod;
			else
				return property.SetMethod;
		}
	}


}
