using System;
using NUnit.Framework;

namespace SemVerTests
{
    /// <summary>
    /// This is similar to the four-part versioning tests but adds assembly redirects into the code
    /// </summary>
    class AssemblyResolveEventTests
    {
        [Test]
        public void AssemblyResolveEventCanFixTheFourPartVersioningCase()
        {
            var c_SharedLibrary_source = @"public class Utils {}";
            var c_CompareEngineSource = @"public class Engine { public void DoWork() {new Utils(); }}";
            var c_Ui_source = @"using System; using System.Reflection;
public class AssemblyResolver : IDisposable
{
    public AssemblyResolver() { AppDomain.CurrentDomain.AssemblyResolve += Load; } 
    public Assembly Load(object o, ResolveEventArgs e) { return Assembly.LoadFrom(""SharedLibrary_redirect_new.dll"");}
    public void Dispose() { AppDomain.CurrentDomain.AssemblyResolve -= Load; }
}
public class Ui { public void Show() { using (new AssemblyResolver()) new Engine().DoWork(); }}";
            var compiler = new Compiler();

            // Compile the engine against the shared library
            var oldSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source, version: new Version(18, 0, 0, 123)).Rename("SharedLibrary_redirect_old.dll");
            var engine = compiler.CompileAndCopyLocal("Engine", c_CompareEngineSource, oldSharedLibrary);

            // Meanwhile, the shared library is rebuilt
            var newSharedLibrary = compiler.CompileAndCopyLocal("SharedLibrary", c_SharedLibrary_source, version: new Version(18, 0, 0, 456)).Rename("SharedLibrary_redirect_new.dll");

            // The engine's nuspec indicates it's compatible with library versions [18.0.0.0, 19.0.0.0)
            // So a nuget update in the UI solution will install the newer shared library
            var productUi = compiler.CompileAndCopyLocal("UI", c_Ui_source, engine);

            // SSMS comes along and loads everything
            newSharedLibrary.CreateInstance("Utils");
            engine.CreateInstance("Engine");
            
            var ui = productUi.CreateInstance("Ui");
            
            Assert.DoesNotThrow(() => ui.Show());
        }
    }
}
