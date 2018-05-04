using System;
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
    }
}
