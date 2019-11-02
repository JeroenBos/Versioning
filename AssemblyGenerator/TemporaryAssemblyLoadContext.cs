using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Versioning
{
	public class TemporaryAssemblyLoadContext : AssemblyLoadContext
	{
		protected override Assembly Load(AssemblyName assemblyName)
		{
			throw new NotImplementedException("For now we only load from stream, so this method is unnecessary");
		}
	}
}
