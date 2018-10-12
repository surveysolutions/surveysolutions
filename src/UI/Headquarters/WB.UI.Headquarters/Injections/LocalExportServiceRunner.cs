using System;
using System.Diagnostics;
using System.IO;
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
        }

        private async Task StartService()
        {
            var tsc = new TaskCompletionSource<bool>();

            var processStartInfo = new ProcessStartInfo(serviceExe, "--console")
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
                logger.Debug(e.Data);
            }

            void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                tsc?.TrySetException(new Exception("An error occur while starting Export Service. View logs for details"));
                logger.Error(e.Data);
            }

            await tsc.Task;
            tsc = null;
        }

        public void Run()
        {
            if (!canRun) return;
            
            lock (locker)
            {
                var pidFile = Path.Combine(servicePath, "pid");

                if (File.Exists(pidFile))
                {
                    return;
                }

                StartService().Wait();
            }

            return;
        }
    }
}
