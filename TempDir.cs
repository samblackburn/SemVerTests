using System;
using System.IO;

namespace SemVerTests
{
    public class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string this[string relativePath]
        {
            get => File.ReadAllText(System.IO.Path.Combine(Path, relativePath));
            set => File.WriteAllText(System.IO.Path.Combine(Path, relativePath), value);
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }

        public string PathTo(string relative) => System.IO.Path.Combine(Path, relative);
    }
}