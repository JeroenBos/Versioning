using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Versioning.Tests
{
	public static class Extensions
	{
		public static CompatiblityIssueCollector ToSingleton<T>(this ICompatibilityIssueRaiser<T> raiser) where T : class
		{
			return new CompatiblityIssueCollector(new ICompatibilityIssueRaiser[] { raiser });
		}
	}
}
