﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Issues
{
	public class MissingFieldIssue : ICompatibilityIssue
	{
		public FieldInfo MissingField { get; }
		public MissingFieldIssue(FieldInfo missingField)
		{
			if (missingField == null) throw new ArgumentNullException(nameof(missingField));

			this.MissingField = missingField;
		}
	}
}
