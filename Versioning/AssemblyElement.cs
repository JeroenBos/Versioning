
namespace Versioning
{

	public enum AssemblyElement
	{
		Class,
		Struct,

	}

	public static class AssemblyElementExtensions
	{
		public static AssemblyElement ToAssemblyElement()
		{
			return AssemblyElement.Class;
		}
	}
}
