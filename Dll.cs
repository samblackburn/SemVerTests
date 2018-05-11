using System;
using System.IO;
using System.Reflection;

namespace SemVerTests
{
    internal class Dll
    {
        public readonly string FilePath;
        public readonly Version Version;

        public Dll(string filePath, Version version) :
            this(filePath, version, Path.GetFileNameWithoutExtension(filePath))
        {
        }

        public Dll(string filePath, Version version, string assemblyName)
        {
            FilePath = filePath;
            Version = version;
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
        public string FileName => Path.GetFileName(FilePath);
        public string Folder => Path.GetDirectoryName(FilePath);

        public dynamic CreateInstance(string typeName)
        {
            return Assembly.LoadFrom(FilePath).CreateInstance(typeName);
        }

        public Dll CopyTo(TempDir tempDir)
        {
            var destination = tempDir.PathTo(FileName);
            File.Copy(FilePath, destination);
            return new Dll(destination, Version, AssemblyName);
        }

        public Dll Rename(string newName)
        {
            var newFilePath = Path.Combine(Folder, newName);
            if (File.Exists(newFilePath)) File.Delete(newFilePath);
            File.Move(FilePath, newFilePath);
            return new Dll(newFilePath, Version, AssemblyName);
        }
    }
}