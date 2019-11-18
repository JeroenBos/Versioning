using Microsoft.CodeAnalysis;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Versioning.UsageDetector.Tests
{
	public readonly struct NamedMetadataReference
	{
		public PortableExecutableReference Reference { get; }
		public AssemblyNameReference Name { get; }

		public NamedMetadataReference(PortableExecutableReference reference, AssemblyNameReference name) => (Reference, Name) = (reference, name);

		public void Deconstruct(out PortableExecutableReference reference, out AssemblyNameReference name) => (reference, name) = (Reference, Name);
		public static implicit operator NamedMetadataReference((PortableExecutableReference Reference, AssemblyNameReference Name) tuple) => new NamedMetadataReference(tuple.Reference, tuple.Name);
	}
}
