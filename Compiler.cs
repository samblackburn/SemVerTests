using System;
using System.IO;
using NUnit.Framework;
using SACrunch;

namespace SemVerTests
{
    internal class Compiler
    {
        private const string c_MsBuild = @"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe";

        internal static string CompileAndCopyLocal(string cs)
        {
            var assemblyName = Path.GetRandomFileName();
            using (var tempDir = new TempDir())
            {
                tempDir["class.cs"] = cs;
                tempDir[assemblyName + ".csproj"] = Csproj("class.cs");
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
    }
}