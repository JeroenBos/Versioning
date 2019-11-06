using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Versioning.UsageDetector
{
	public static class ListAllReferences
	{
		public static IEnumerable<MemberReference> GetAllMemberReferences(AssemblyDefinition assembly)
		{
			// it suffices to only check in method bodies, because property/event accessors are implemented as methods as well,
			// and e.g. field initializers are in the constructor. Local functions are also lowered to nonlocal functions, so it's fine
			return from TypeDefinition type in assembly.MainModule.Types
				   from MethodDefinition method in type.Methods
				   where method.HasBody
				   from Instruction instruction in method.Body.Instructions
				   where instruction.Operand is MemberReference reference
				   select instruction.Operand as MemberReference; ; // Hm. The name 'reference' does not exist in the current context
		}
	}
}
