using System;
using System.Diagnostics;
using System.IO;

namespace SACrunch
{
    public class ProcessRunner
    {
        public static string RunProcess(string pathToExe, string arguments, string workingDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(pathToExe, arguments)
                {
                    WorkingDirectory = workingDirectory,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            using (process)
            {
                process.Start();

                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();

                if (!process.WaitForExit(10000))
                {
                    throw new TimeoutException(string.Format("Failed to run {0} {1}", pathToExe, arguments));
                }

                return output.Result + error.Result;
            }
        }
    }

    public class ExitCodeException : Exception
    {
        public ExitCodeException(string pathToExe, string arguments, int exitCode, string result)
            : base(string.Format("{0} {1} returned exit code {2}{3}{4}", pathToExe, arguments, exitCode, Environment.NewLine, result))
        {
            Exe = Path.GetFileName(pathToExe);
            Result = result;
            ExitCode = exitCode;
        }

        public readonly string Exe;
        public readonly string Result;
        public readonly int ExitCode;
    }
}