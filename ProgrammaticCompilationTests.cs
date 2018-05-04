using System.CodeDom.Compiler;
using Microsoft.CSharp;
using NUnit.Framework;

namespace SemVerTests
{
    public class ProgrammaticCompilationTests
    {
        [Test]
        public void CanCompileAndLoadADll()
        {
            var cs = @"public class Foo{public int Version() {return 1;}}";
            var assembly = new CSharpCodeProvider()
                .CompileAssemblyFromSource(new CompilerParameters(), cs)
                .CompiledAssembly;
            dynamic foo = assembly.CreateInstance("Foo");
            Assert.AreEqual(1, foo.Version());
        }

        [Test]
        public void CanCompileADllAgainstADependency()
        {
            var compiler = new CSharpCodeProvider();

            var cs1 = @"public class Foo{public int Version() {return 1;}}";
            var assembly1 = compiler
                .CompileAssemblyFromSource(new CompilerParameters(new string[0], "foo.dll"), cs1)
                .CompiledAssembly;
            var cs2 = @"public class Bar{public int AccessFoo() {return new Foo().Version();}}";
            var assembly2 = compiler
                .CompileAssemblyFromSource(new CompilerParameters(new[] {"foo.dll"}, "bar.dll"), cs2);
            CollectionAssert.IsEmpty(assembly2.Errors);
            dynamic bar = assembly2.CompiledAssembly.CreateInstance("Bar");
            Assert.AreEqual(1, bar.AccessFoo());
        }
    }
}
