using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Versioning.Issues
{
	public class MissingPropertyIssue : IMissingMemberCompatibilityIssue
	{
		public PropertyDefinition MissingProperty { get; }
		public MissingPropertyIssue(PropertyDefinition property)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));

			this.MissingProperty = property;
		}

		IMemberDefinition IMissingMemberCompatibilityIssue.MissingMember => MissingProperty;
	}


	public class MissingAccessorIssue : MissingMemberIssue
	{
		public PropertyAccessor Accessor { get; }
		public PropertyDefinition Property { get; }
		public MissingAccessorIssue(PropertyDefinition property, PropertyAccessor missingAccessor)
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
		public static PropertyAccessor GetAccessorsEnum(this PropertyDefinition propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

			return (propertyInfo.GetMethod != null ? PropertyAccessor.Get : PropertyAccessor.None)
				| (propertyInfo.SetMethod != null ? PropertyAccessor.Set : PropertyAccessor.None);
		}
		public static MethodDefinition? Select(this PropertyDefinition property, PropertyAccessor accessor)
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
