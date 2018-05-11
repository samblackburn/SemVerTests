using System;
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
            var assembly = Compiler.CompileAndCopyLocal(nameof(CanCompileCode), @"public class Foo{public int Version() {return 1;}}");

            Assert.AreEqual(1, ((dynamic) Assembly.LoadFile(assembly).CreateInstance("Foo")).Version());
        }

        /// <summary>
        /// Assembly.LoadFile() doesn't apply references correctly
        /// </summary>
        [Test]
        public void FailToReferenceADll()
        {
            var assembly1 = Compiler.CompileAndCopyLocal(nameof(FailToReferenceADll) + "Foo", @"public class Foo{public int Version() {return 1;}}");
            var assembly2 = Compiler.CompileAndCopyLocal(nameof(FailToReferenceADll) + "Bar", @"public class Bar{public int AccessFoo() {return new Foo().Version();}}", assembly1);

            dynamic foo = Assembly.LoadFile(assembly1).CreateInstance("Foo");
            dynamic bar = Assembly.LoadFile(assembly2).CreateInstance("Bar");

            Assert.AreEqual(1, foo.Version());
            var ex = Assert.Throws<FileNotFoundException>(() => bar.AccessFoo());

            StringAssert.Contains(Path.GetFileNameWithoutExtension(assembly1), ex.Message);
        }

        /// <summary>
        /// Assembly.LoadFrom() will load dependencies
        /// </summary>
        [Test]
        public void SuccessfullyReferenceADll()
        {
            var assembly1 = Compiler.CompileAndCopyLocal(nameof(SuccessfullyReferenceADll) + "Foo", @"public class Foo{public int Version() {return 1;}}");
            var assembly2 = Compiler.CompileAndCopyLocal(nameof(SuccessfullyReferenceADll) + "Bar", @"public class Bar{public int AccessFoo() {return new Foo().Version();}}", assembly1);

            dynamic foo = Assembly.LoadFrom(assembly1).CreateInstance("Foo");
            dynamic bar = Assembly.LoadFrom(assembly2).CreateInstance("Bar");

            Assert.AreEqual(1, bar.AccessFoo());
        }

        /// <summary>
        /// Here we compile Bar against Foo v1, then switch it for Foo v2 at runtime
        /// </summary>
        [Test]
        public void UpgradeAfterCompile()
        {
            var fooV1 = Compiler.CompileAndCopyLocal(nameof(UpgradeAfterCompile) + "Foo", @"public class Foo{public int Version() {return 1;}}");
            var barV1 = Compiler.CompileAndCopyLocal(nameof(UpgradeAfterCompile) + "Bar", @"public class Bar{public int AccessFoo() {return new Foo().Version();}}", fooV1);
            var fooV2 = Compiler.CompileAndCopyLocal(nameof(UpgradeAfterCompile) + "Foo", @"public class Foo{public int Version() {return 2;}}");

            dynamic foo = Assembly.LoadFrom(fooV2).CreateInstance("Foo");
            dynamic bar = Assembly.LoadFrom(barV1).CreateInstance("Bar");

            Assert.AreEqual(2, bar.AccessFoo());
        }

        /// <summary>
        /// Here we compile Bar against Foo v1, then switch it for Foo v2 at runtime
        /// </summary>
        [Test]
        public void MethodNotFound()
        {
            var fooV1 = Compiler.CompileAndCopyLocal(nameof(MethodNotFound) + "Foo", @"public class Foo{public int Deprecated() {return 1;}}");
            var barV1 = Compiler.CompileAndCopyLocal(nameof(MethodNotFound) + "Bar", @"public class Bar{public int AccessFoo() {return new Foo().Deprecated();}}", fooV1);
            var fooV2 = Compiler.CompileAndCopyLocal(nameof(MethodNotFound) + "Foo", @"public class Foo{}");

            dynamic foo = Assembly.LoadFrom(fooV2).CreateInstance("Foo");
            dynamic bar = Assembly.LoadFrom(barV1).CreateInstance("Bar");

            Assert.Throws<MissingMethodException>(() => bar.AccessFoo());
        }
    }
}
