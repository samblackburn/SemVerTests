using System;
using System.IO;
using NUnit.Framework;

namespace SemVerTests
{
    class FourPartVersioningTests
    {
        [Test]
        public void NugetUpgradesCanBreakASingleProduct()
        {
            var c_SharedLibrary_source = @"public class Utils {}";
            var c_CompareEngineSource = @"public class Engine { public void DoWork() { new Utils(); } }";
            var c_Ui_source = @"public class Ui { public void Show() { new Engine().DoWork(); } }";

            var compiler = new Compiler();

            // Compile the engine against the shared library
            var oldSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source, version: new Version(18, 0, 0, 123)).Rename("SharedLibrary_old.dll");
            var engine = compiler.CompileAndCopyLocal("Engine", c_CompareEngineSource, oldSharedLibrary);
            
            // Meanwhile, the shared library is rebuilt
            var newSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source, version: new Version(18, 0, 0, 456)).Rename("SharedLibrary_new.dll");

            // The engine's nuspec indicates it's compatible with library versions [18.0.0.0, 19.0.0.0)
            // So a nuget update in the UI solution will install the newer shared library
            var productUi = compiler.CompileAndCopyLocal("UI", c_Ui_source, engine);

            // SSMS comes along and loads everything
            newSharedLibrary.CreateInstance("Utils");
            engine.CreateInstance("Engine");
            var ui = productUi.CreateInstance("Ui");

            // But sadly...
            var ex = Assert.Throws<FileNotFoundException>(() => ui.Show());
            Console.WriteLine(ex);
        }
    }
}
