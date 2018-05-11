using System;
using NUnit.Framework;

namespace SemVerTests
{
    class MajorVersionOnlyTests
    {
        [Test]
        public void ApiChangesWithoutMajorVersionBumpCauseCrashesWhenSeveralProductsAreInstalled()
        {
            var c_SharedLibrary_source_old = @"public class Utils { }";
            var c_SharedLibrary_source_new = @"public class Utils { public void NewMethod() {} }";
            var c_Prompt_source = @"public class Ui { public void Show() { new Utils().NewMethod(); } }";
            var c_Soc_source = "public class Ui { public void Show() { } }";

            var compiler = new Compiler();

            // Compile SOC against shared library version 18.1
            var oldSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source_old, version: new Version(18, 0, 0, 0)).Rename("SharedLib_old.dll");
            var soc = compiler.CompileAndCopyLocal("Soc", c_Soc_source, oldSharedLibrary);

            // Compile Prompt against shared library version 18.2
            var newSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source_new, version: new Version(18, 0, 0, 0)).Rename("SharedLib_new.dll");
            var prompt = compiler.CompileAndCopyLocal("Prompt", c_Prompt_source, newSharedLibrary);
            
            // SSMS comes along and loads everything, unfortunately in the wrong order
            oldSharedLibrary.CreateInstance("Utils");
            newSharedLibrary.CreateInstance("Utils");
            soc.CreateInstance("UI");
            var ui = prompt.CreateInstance("Ui");

            // But sadly...
            var ex = Assert.Throws<MissingMethodException>(() => ui.Show());
            Console.WriteLine(ex);
        }
    }
}
