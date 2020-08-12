using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace WB.Services.Export.Host
{
    public class StartupBlocker
    {
        private static FileStream? pid;

        private const string PidFileName = ".pid";

        // pid file - is a file that is exists only while process is alive and contains own process id
        public void OpenPIDFile()
        {
            if (!IsThereIsAliveRunningExportProcess())
            {
                pid = new FileStream(PidFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096,
                    FileOptions.DeleteOnClose);
                var writer = new StreamWriter(pid);

                writer.WriteLine(Process.GetCurrentProcess().Id);
                writer.Flush();
            }
        }

        private bool IsThereIsAliveRunningExportProcess()
        {
            if (File.Exists(PidFileName))
            {
                var fs = TryOpenFile(PidFileName);

                if (fs==null)
                {
                    // if we cannot open pid file, then someone is using it
                    return true;
                }

                try
                {
                    using var sr = new StreamReader(fs);

                    var pidFileValue = sr.ReadToEnd();

                    if (int.TryParse(pidFileValue, out var processId))
                    {
                        if (TryGetProcess(processId, out var process))
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            var currentProcessName = Process.GetCurrentProcess().MainModule.FileName;

                            if (!process.HasExited && process.MainModule?.FileName == currentProcessName)
                            {
                                // this should be not an allowed situation. There is an existing process with 
                                // same name as ours, with same path as ours but is not locked. 
                                // lets assume this is a valid export service running
                                return true;
                            }
                        }
                    }
                }
                finally
                {
                    fs.Dispose();
                }

                File.Delete(PidFileName);
            }

            return false;
        }

        private bool TryGetProcess(int processId, [MaybeNullWhen(returnValue: false)] out Process process)
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

        private FileStream? TryOpenFile(string file)
        {
            try
            {
                return new FileStream(file, FileMode.Open);
            }
            catch
            {
                return null;
            }
        }
    }
}
