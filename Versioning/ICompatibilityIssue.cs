﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Versioning
{
	public interface ICompatibilityIssue
	{

	}
	public class MissingTypeIssue : ICompatibilityIssue
	{
		public Type MissingType { get; }
		public MissingTypeIssue(Type missingType)
		{
			if (missingType == null) throw new ArgumentNullException(nameof(missingType));

			this.MissingType = missingType;
		}
	}
	public class MissingMemberIssue : ICompatibilityIssue
	{

	}

	public class MissingPropertyIssue : MissingMemberIssue
	{

	}
}
