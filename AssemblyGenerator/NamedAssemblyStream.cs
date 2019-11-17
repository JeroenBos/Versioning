using System.IO;

readonly struct NamedAssemblyStream
{
	public string Name { get; }
	public Stream Stream { get; }
	public NamedAssemblyStream(string name, Stream stream) => (Name, Stream) = (name, stream);
	public static implicit operator NamedAssemblyStream((string, Stream) tuple)
	{
		return new NamedAssemblyStream(tuple.Item1, tuple.Item2);
	}
}
