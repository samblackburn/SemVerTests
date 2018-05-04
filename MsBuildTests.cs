using System.IO;
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

        [Test]
        public void ReferenceADll()
        {
            var assembly1 = Compiler.CompileAndCopyLocal(@"public class Foo{public int Version() {return 1;}}");
            var assembly2 = Compiler.CompileAndCopyLocal(@"public class Bar{public int AccessFoo() {return new Foo().Version();}}", assembly1);

            dynamic foo = Assembly.LoadFile(assembly1).CreateInstance("Foo");
            dynamic bar = Assembly.LoadFile(assembly2).CreateInstance("Bar");

            Assert.AreEqual(1, foo.Version());
            var ex = Assert.Throws<FileNotFoundException>(() => bar.AccessFoo());

            StringAssert.Contains(Path.GetFileNameWithoutExtension(assembly1), ex.Message);
        }
    }
}
