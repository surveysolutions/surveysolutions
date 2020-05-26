using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
   /// <summary>
    /// Contains methods for running commands and reading standard output (stdout).
    /// </summary>
    public static class Command
    {
        /// <summary>
        /// Runs a command.
        /// By default, the command line is echoed to standard error (stderr).
        /// </summary>
        /// <param name="name">The name of the command. This can be a path to an executable file.</param>
        /// <param name="args">The arguments to pass to the command.</param>
        /// <exception cref="CommandException">The command exited with non-zero exit code.</exception>
        /// <remarks>
        /// By default, the resulting command line and the working directory (if specified) are echoed to standard error (stderr).
        /// To suppress this behavior, provide the <paramref name="noEcho"/> parameter with a value of <c>true</c>.
        /// </remarks>
        public static void Run(string name, string args = null, Action<string> outputDataReceived = null)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(name, args);
            process.OutputDataReceived += (sender, e) => outputDataReceived?.Invoke(e.Data);

            process.Start();

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
        /// <returns>A <see cref="string"/> representing the contents of standard output (stdout).</returns>
        /// <remarks>
        /// By default, the resulting command line and the working directory (if specified) are echoed to standard error (stderr).
        /// To suppress this behavior, provide the <paramref name="noEcho"/> parameter with a value of <c>true</c>.
        /// </remarks>
        public static string Read(string name, string args = null, Action<string> outputDataReceived = null)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(name, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.OutputDataReceived += (sender, e) => outputDataReceived?.Invoke(e.Data);

            var runProcess = process.RunAsync(true);
            var readOutput = process.StandardOutput.ReadToEndAsync();

            Task.WaitAll(runProcess, readOutput);

            if (process.ExitCode != 0)
            {
                var errorOutput = process.StandardError.ReadToEndAsync();
                Task.WaitAll(errorOutput);
                throw new NonZeroExitCodeException(process.ExitCode);
            }

            return readOutput.Result;
        }
        
        public static Task RunAsync(this Process process, bool noEcho, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<object>(cancellationToken);
            process.Exited += (s, e) => tcs.SetResult(null);
            process.EnableRaisingEvents = true;
            
            process.EchoAndStart(noEcho);

            cancellationToken.Register(() =>
            {
                process.CloseMainWindow();
                process.Kill();                
            });

            return tcs.Task;
        }

        private static void EchoAndStart(this Process process, bool noEcho)
        {
            if (!noEcho)
            {
                var message = $"{(process.StartInfo.WorkingDirectory == "" ? "" : $"Working directory: {process.StartInfo.WorkingDirectory}{Environment.NewLine}")}{process.StartInfo.FileName} {process.StartInfo.Arguments}";
                Console.Error.WriteLine(message);
            }

            process.Start();
        }
    }

   public class NonZeroExitCodeException : Exception
   {
       public int ProcessExitCode { get; }

       public NonZeroExitCodeException(in int processExitCode)
       {
           ProcessExitCode = processExitCode;
       }
   }
}
