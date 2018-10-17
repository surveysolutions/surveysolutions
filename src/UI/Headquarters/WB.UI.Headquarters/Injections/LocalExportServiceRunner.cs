using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Headquarters.Injections
{
    class LocalExportServiceRunner : ILocalExportServiceRunner
    {
        private readonly object locker = new object();
        private readonly string servicePath;
        private readonly string serviceExe;
        private readonly bool canRun;
        private readonly ILogger logger;
        private readonly TaskCompletionSource<bool> tsc;

        public LocalExportServiceRunner(ILogger logger)
        {
            this.logger = logger;

            if (HttpContext.Current != null)
            {
                var httpCtx = HttpContext.Current;
                servicePath = httpCtx.Server.MapPath(@"~/.bin/Export");
                serviceExe = Path.Combine(servicePath, "WB.Services.Export.Host.exe");
                canRun = servicePath != null && serviceExe != null && File.Exists(serviceExe);
            }

            this.tsc = new TaskCompletionSource<bool>();
        }

        private async Task StartService()
        {
            var data = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(ConfigurationManager.ConnectionStrings[@"Postgres"].ConnectionString),
                null, DataProtectionScope.CurrentUser);

            // for local running export service we passing connection string via command line using ProtectedData api
            // so that connection string is no visible via taskmgr.exe
            var processStartInfo = new ProcessStartInfo(serviceExe, $"--console --connectionString={Convert.ToBase64String(data)}")
            {
                WorkingDirectory = servicePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.OutputDataReceived += ProcessOnOutputDataReceived;
            process.ErrorDataReceived += ProcessOnErrorDataReceived;

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                tsc?.TrySetResult(true);
                logger.Info(e.Data);
            }

            void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                tsc?.TrySetException(new Exception(@"An error occur while starting Export Service. View logs for details"));
                logger.Error(e.Data);
            }

            await tsc.Task.ConfigureAwait(false);
        }

        public void Run()
        {
            if (!canRun) return;

            lock (locker)
            {
                if (CheckForRunningExportProcess()) return;

                StartService().Wait(5000); // make sure we are not hang
            }
        }

        private bool CheckForRunningExportProcess()
        {
            var pidFile = Path.Combine(servicePath, "pid");

            if (File.Exists(pidFile))
            {
                if (!TryOpenFile(pidFile, out var fs))
                {
                    return true;
                }

                try
                {
                    using (var sr = new StreamReader(fs))
                    {
                        var pidFileValue = sr.ReadToEnd();

                        if (int.TryParse(pidFileValue, out var processId))
                        {
                            if (TryGetProcess(processId, out var process))
                            {
                                if (!process.HasExited && process.MainModule.FileName == serviceExe)
                                    return true;
                            }
                        }
                    }
                }
                finally
                {
                    fs.Dispose();
                }

                File.Delete(pidFile);
            }

            return false;
        }

        private bool TryGetProcess(int processId, out Process process)
        {
            try
            {
                process = Process.GetProcessById(processId);
                return true;
            }
            catch
            {
                process = null;
                return false;
            }
        }

        private bool TryOpenFile(string file, out FileStream stream)
        {
            try
            {
                stream = new FileStream(file, FileMode.Open);
                return true;
            }
            catch
            {
                stream = null;
                return false;
            }
        }
    }
}
