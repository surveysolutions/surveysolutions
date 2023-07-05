using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Infrastructure.Native.Utils
{
   /// <summary>
    /// Contains methods for running commands and reading standard output (stdout).
    /// </summary>
    public static class ConsoleCommand
    {
        /// <summary>
        /// Runs a command.
        /// By default, the command line is echoed to standard error (stderr).
        /// </summary>
        /// <param name="name">The name of the command. This can be a path to an executable file.</param>
        /// <param name="args">The arguments to pass to the command.</param>
        /// <exception cref="NonZeroExitCodeException">The command exited with non-zero exit code.</exception>
        public static void Run(string name, string args = null, Action<string> outputDataReceived = null)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(name, args);
            process.OutputDataReceived += (sender, e) => outputDataReceived?.Invoke(e.Data);

            process.Start();
            process.WaitForExit(40_000);

            if (process.ExitCode != 0)
            {
                throw new NonZeroExitCodeException(process.ExitCode);
            }
        }


        /// <summary>
        /// Runs a command and reads standard output (stdout).
        /// By default, the command line is echoed to standard error (stderr).
        /// </summary>
        /// <param name="name">The name of the command. This can be a path to an executable file.</param>
        /// <param name="args">The arguments to pass to the command.</param>
        /// <param name="workingDirectory"></param>
        /// <returns>A <see cref="string"/> representing the contents of standard output (stdout).</returns>
        public static string Read(string name, string args = null, string workingDirectory = null, 
            Action<string> outputDataReceived = null, CancellationToken cancellationToken = default)
        {
            using var process = new Process();
            var startInfo = new ProcessStartInfo(name, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory
            };

            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => outputDataReceived?.Invoke(e.Data);

            var tcs = new TaskCompletionSource<object>(cancellationToken);
            process.Exited += (s, e) => tcs.SetResult(null);
            process.EnableRaisingEvents = true;
            
            process.Start();

            cancellationToken.Register(() =>
            {
                process.CloseMainWindow();
                process.Kill();                
            });
            
            var readOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var readError = process.StandardError.ReadToEndAsync(cancellationToken);

            Task.WaitAll(tcs.Task, readOutput);

            if (process.ExitCode != 0)
            {
                var errorOutput = process.StandardError.ReadToEndAsync(cancellationToken);
                Task.WaitAll(errorOutput);
                throw new NonZeroExitCodeException(process.ExitCode, readError.Result);
            }

            string result = readOutput.Result;
            if(string.IsNullOrEmpty(result))
            {
                return readError.Result;
            }

            return result;
        }
    }
}
