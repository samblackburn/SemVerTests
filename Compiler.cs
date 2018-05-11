using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using SACrunch;

namespace SemVerTests
{
    internal class Compiler
    {
        private readonly string m_TestName;

        public Compiler([CallerMemberName] string uniqueTestName = null)
        {
            m_TestName = uniqueTestName;
        }

        private const string c_MsBuild = @"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe";

        internal Dll CompileAndCopyLocal(string assemblySuffix, string cs, Dll reference = null)
        {
            var assemblyName = $"{m_TestName}_{assemblySuffix}";

            using (var tempDir = new TempDir())
            {
                if (reference != null)
                {
                    var copyOfReferenceDll = reference.CopyTo(tempDir);
                    tempDir[assemblyName + ".csproj"] = Csproj("class.cs", copyOfReferenceDll);
                }
                else
                {
                    tempDir[assemblyName + ".csproj"] = Csproj("class.cs");
                }

                tempDir["class.cs"] = "using System.Reflection;\r\n[assembly: AssemblyVersion(\"1.2.3.4\")]\r\n" +cs;
                RunMsbuild(tempDir, assemblyName + ".csproj");

                var dll = tempDir.PathTo($@"bin\Debug\{assemblyName}.dll");
                FileAssert.Exists(dll);
                File.Copy(dll, $"{assemblyName}.dll", true);
                return new Dll(Path.Combine(Directory.GetCurrentDirectory(), assemblyName + ".dll"));
            }
        }

        private static void RunMsbuild(TempDir tempDir, string args)
        {
            var buildOutput = ProcessRunner.RunProcess(c_MsBuild, args, tempDir.Path);
            //Console.WriteLine(buildOutput);
            StringAssert.Contains("Build succeeded.", buildOutput);
            StringAssert.DoesNotContain("Build FAILED.", buildOutput);
        }

        private static string Csproj(string sourceFile, string referenceDll = null)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
    <SignAssembly>true</SignAssembly>
    <AssemblyOriginatorKeyFile>\\red-gate.com\Files\RG_Build_Key\RedGate.snk</AssemblyOriginatorKeyFile>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""{sourceFile}"" />
    {ReferenceForCsproj(referenceDll)}
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>";
        }

        private static string ReferenceForCsproj(string referenceDll)
        {
            if (referenceDll == null)
            {
                return null;
            }

            return $@"<Reference Include=""{Path.GetFileNameWithoutExtension(referenceDll)}, Version=1.2.3.4, Culture=neutral, PublicKeyToken=7f465a1c156d4d57"">
      <HintPath>{referenceDll}</HintPath>
    </Reference>";
        }
    }
}