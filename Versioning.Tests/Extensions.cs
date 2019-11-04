using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Tests
{
	public static class Extensions
	{
		public static CompatiblityIssueRaiserVisitor ToSingleton<T>(this ICompatiblityIssueRaiser<T> raiser) where T : MemberInfo
		{
			return new CompatiblityIssueRaiserVisitor(new ICompatiblityIssueRaiser[] { raiser });
		}
	}
}
