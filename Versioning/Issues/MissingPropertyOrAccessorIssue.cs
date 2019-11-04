using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Issues
{
	public class MissingPropertyOrAccessorIssue : ICompatibilityIssue
	{
		public PropertyInfo Property { get; }
		public PropertyAccessor MissingAccessors { get; }
		public MissingPropertyOrAccessorIssue(PropertyInfo property, PropertyAccessor missingAccessors)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));

			this.Property = property;
			this.MissingAccessors = missingAccessors;
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
	}
}
