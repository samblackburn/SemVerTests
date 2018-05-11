using System;
using NUnit.Framework;

namespace SemVerTests
{
    class MajorVersionOnlyTests
    {
        [Test]
        public void ApiChangesWithoutMajorVersionBumpCauseCrashesWhenSeveralProductsAreInstalled()
        {
            var c_SharedLibrary_source_old = @"public class Utils { public void Deprecated() {} }";
            var c_SharedLibrary_source_new = @"public class Utils {}";
            var c_Ui_source = @"public class Ui { public void Show() { new Utils().Deprecated(); } }";

            var compiler = new Compiler();

            // Compile the engine against the shared library
            var oldSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source_old, version: new Version(18, 0, 0, 0)).Rename("SharedLib_old.dll");
            var soc = compiler.CompileAndCopyLocal("Soc", c_Ui_source, oldSharedLibrary);
            var newSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source_new, version: new Version(18, 0, 0, 0)).Rename("SharedLib_new.dll");
            var prompt = compiler.CompileAndCopyLocal("Prompt", "", newSharedLibrary);
            
            // SSMS comes along and loads everything
            newSharedLibrary.CreateInstance("Utils");
            oldSharedLibrary.CreateInstance("Engine");
            prompt.CreateInstance("UI");
            var ui = soc.CreateInstance("Ui");

            // But sadly...
            var ex = Assert.Throws<MissingMethodException>(() => ui.Show());
            Console.WriteLine(ex);
        }
    }
}
