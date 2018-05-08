using System;
using System.IO;
using NUnit.Framework;
using SACrunch;

namespace SemVerTests
{
    internal class Compiler
    {
        private const string c_MsBuild = @"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe";
        private static int s_AssemblyNumber = 0;

        internal static string CompileAndCopyLocal(string cs, string reference = null, string assemblyName = null)
        {
            assemblyName = assemblyName ?? "Assembly" + ++s_AssemblyNumber;
            using (var tempDir = new TempDir())
            {
                if (reference != null)
                {
                    var copyOfReferenceDll = tempDir.PathTo(Path.GetFileName(reference));
                    File.Copy(reference, copyOfReferenceDll);
                    tempDir[assemblyName + ".csproj"] = CsprojWithReference("class.cs", copyOfReferenceDll);
                }
                else
                {
                    tempDir[assemblyName + ".csproj"] = Csproj("class.cs");
                }

                tempDir["class.cs"] = cs;
                RunMsbuild(tempDir, assemblyName + ".csproj");

                var dll = tempDir.PathTo($@"bin\Debug\{assemblyName}.dll");
                FileAssert.Exists(dll);
                File.Copy(dll, $"{assemblyName}.dll", true);
                return Path.Combine(Directory.GetCurrentDirectory(), assemblyName + ".dll");
            }
        }

        private static void RunMsbuild(TempDir tempDir, string args)
        {
            var buildOutput = ProcessRunner.RunProcess(c_MsBuild, args, tempDir.Path);
            Console.WriteLine(buildOutput);
            StringAssert.Contains("Build succeeded.", buildOutput);
            StringAssert.DoesNotContain("Build FAILED.", buildOutput);
        }

        private static string Csproj(string sourceFile)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""{sourceFile}"" />
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>";
        }

        private static string CsprojWithReference(string sourceFile, string referenceDll)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""{sourceFile}"" />
     <Reference Include=""{Path.GetFileNameWithoutExtension(referenceDll)}"">
      <HintPath>{referenceDll}</HintPath>
    </Reference>
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>";
        }
    }
}