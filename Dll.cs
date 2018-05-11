using System.IO;
using System.Reflection;

namespace SemVerTests
{
    internal class Dll
    {
        public readonly string FilePath;

        public Dll(string filePath)
        {
            FilePath = filePath;
        }

        public string FileName => Path.GetFileName(FilePath);

        public dynamic CreateInstance(string typeName)
        {
            return Assembly.LoadFrom(FilePath).CreateInstance(typeName);
        }
    }
}