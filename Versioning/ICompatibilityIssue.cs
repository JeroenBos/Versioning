using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning
{
	public interface ICompatibilityIssue
	{

	}
	public interface IMissingMemberCompatibilityIssue : ICompatibilityIssue
	{
		MemberInfo MissingMember { get; }
	}
}
