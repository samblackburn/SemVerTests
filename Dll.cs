using System;
using System.IO;
using System.Reflection;

namespace SemVerTests
{
    internal class Dll
    {
        public readonly string FilePath;
        public readonly Version Version;
        
        public Dll(string filePath, Version version)
        {
            FilePath = filePath;
            Version = version;
        }

        public string FileName => Path.GetFileName(FilePath);

        public dynamic CreateInstance(string typeName)
        {
            return Assembly.LoadFrom(FilePath).CreateInstance(typeName);
        }

        public Dll CopyTo(TempDir tempDir)
        {
            var destination = tempDir.PathTo(FileName);
            File.Copy(FilePath, destination);
            return new Dll(destination, Version);
        }
    }
}