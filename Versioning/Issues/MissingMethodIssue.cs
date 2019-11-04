﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Issues
{
	public class MissingMethodIssue : ICompatibilityIssue
	{
		public MethodInfo MissingMethod { get; }
		public MissingMethodIssue(MethodInfo missingMethod)
		{
			if (missingMethod == null) throw new ArgumentNullException(nameof(missingMethod));

			this.MissingMethod = missingMethod;
		}
	}
}
