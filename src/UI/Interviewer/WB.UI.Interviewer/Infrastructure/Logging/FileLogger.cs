using System;
using System.IO;
using System.Threading;
using Java.Lang;
using MvvmCross.Platform;
using MvvmCross.Platform.Exceptions;
using WB.Core.GenericSubdomains.Portable.Services;
using Exception = System.Exception;

namespace WB.UI.Interviewer.Infrastructure.Logging
{
    public class FileLogger : ILogger
    {
        private static readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
        private readonly string LogFilePath;

        public FileLogger(string pathToLogFile)
        {
            this.LogFilePath = pathToLogFile;

#if DEBUG
            this.IsDebugEnabled = true;
            this.IsInfoEnabled = true;
            this.IsWarnEnabled = true;
#else
            this.IsDebugEnabled = false;
            this.IsInfoEnabled = false;
            this.IsWarnEnabled = false;
#endif
        }

        public void Debug(string message, Exception exception = null)
        {
            if (!this.IsDebugEnabled) return;

            this.WriteLogMessage("Debug", exception, message);
        }
       
        public void Info(string message, Exception exception = null)
        {
            if (!this.IsInfoEnabled) return;

            this.WriteLogMessage("Info", exception, message);
        }
        
        public void Warn(string message, Exception exception = null)
        {
            if (!this.IsWarnEnabled) return;

            this.WriteLogMessage("Warning", exception, message);
        }

        public void Error(string message, Exception exception = null)
        {
            Mvx.Error("Sync failed with exception {0}", exception != null ? exception.ToLongString(): "" );
            this.WriteLogMessage("Error", exception, message);
        }

        public void Fatal(string message, Exception exception = null)
        {
            this.WriteLogMessage("Fatal", exception, message);
        }
        
        public bool IsDebugEnabled { get; }
        public bool IsInfoEnabled { get; }
        public bool IsWarnEnabled { get; }

        private void WriteLogMessage(string type, Exception exception, string message)
        {
            rwl.EnterWriteLock();
            try
            {
                using (var fileStream = File.AppendText(this.LogFilePath))
                {
                    fileStream.WriteLine(exception != null
                        ? $"{DateTime.UtcNow} {type} {message} {Throwable.FromException(exception)}"
                        : $"{DateTime.UtcNow} {type} {message}");
                }
            }
            finally
            {
                rwl.ExitWriteLock();
            }
        }
    }
}