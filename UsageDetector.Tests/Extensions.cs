using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Versioning.UsageDetector.Tests
{
	public static class Extensions
	{
		public static AsyncTestDelegate? WrapEntryPoint(this Assembly assembly)
		{
			if (assembly.EntryPoint == null)
				return null;

			if (assembly.Location == null)
				throw new ArgumentException("The assembly must have a path");

			return () => Process.Start(assembly.Location).WaitForExitAsync();
		}
	}
}
