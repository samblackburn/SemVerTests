using System.Reflection;
using NUnit.Framework;

namespace SemVerTests
{
    class MsBuildTests
    {
        [Test]
        public void CanCompileCode()
        {
            var assembly = Compiler.CompileAndCopyLocal(@"public class Foo{public int Version() {return 1;}}");

            Assert.AreEqual(1, ((dynamic) Assembly.LoadFile(assembly).CreateInstance("Foo")).Version());
        }
    }
}
