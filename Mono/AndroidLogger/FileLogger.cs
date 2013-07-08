using System;
using System.IO;
using Java.Lang;
using WB.Core.GenericSubdomains.Logging;
using Environment = Android.OS.Environment;
using Exception = System.Exception;

namespace WB.Core.SharedKernel.Utils.Logging
{
    public class FileLogger : ILogger
    {
        private static readonly string LogFilename = Path.Combine(GetLogDirectory(), "WBCapi.log.txt");
        
        private const string Tag = "Android.WBCapi";

        private const string CAPI = "WBCapi";

        private static string GetLogDirectory()
        {
            string extStorage = Environment.ExternalStorageDirectory.AbsolutePath;
            if (Directory.Exists(extStorage))
            {
                extStorage = Path.Combine(extStorage, CAPI);
                if (!Directory.Exists(extStorage))
                {
                    Directory.CreateDirectory(extStorage);
                }
            }
            else
            {
                extStorage = global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.Personal);
            }
            return extStorage;
        }

        public FileLogger()
        {
#if DEBUG
            IsDebugEnabled = true;
#else
            IsDebugEnabled = false;
#endif
            IsErrorEnabled = true;
            IsFatalEnabled = true;
            IsInfoEnabled = true;
            IsWarnEnabled = true;
        }

        public void Debug(string message, Exception exception = null)
        {
            if (!this.IsDebugEnabled) return;
            if (exception == null)
            {
                this.WriteLogMessage(Tag, LogMessageType.Debug, message);
            }
            this.WriteLogMessage(Tag, LogMessageType.Debug, exception.ToThrowable(), message);
        }
       
        public void Info(string message, Exception exception = null)
        {
            if (!this.IsInfoEnabled) return;
            if (exception == null)
            {
                WriteLogMessage(Tag, LogMessageType.Info, message);
            }
            this.WriteLogMessage(Tag, LogMessageType.Info, exception.ToThrowable(), message);
        }
        
        public void Warn(string message, Exception exception = null)
        {
            if (!this.IsWarnEnabled) return;
            if (exception == null)
            {
                this.WriteLogMessage(Tag, LogMessageType.Warning, message);
            }
            this.WriteLogMessage(Tag, LogMessageType.Warning, exception.ToThrowable(), message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
                WriteLogMessage(Tag, LogMessageType.Warning, format, args);
        }

        public void Error(string message, Exception exception = null)
        {
            if (exception==null)
            {
                WriteLogMessage(Tag, LogMessageType.Error, message);
            }
            WriteLogMessage(Tag, LogMessageType.Error, exception.ToThrowable(), message);
        }

        public void Fatal(string message, Exception exception = null)
        {
            if (!this.IsFatalEnabled) return;
            if (exception == null)
            {
                WriteLogMessage(Tag, LogMessageType.Fatal, message);
            }
            this.WriteLogMessage(Tag, LogMessageType.Fatal, exception.ToThrowable(), message);
        }

        public bool IsDebugEnabled { get; private set; }
        public bool IsInfoEnabled { get; private set; }
        public bool IsWarnEnabled { get; private set; }
        public bool IsErrorEnabled { get; private set; }
        public bool IsFatalEnabled { get; private set; }

        private void WriteLogMessage(string tag, string type,  string message)
        {
            using (var s = File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2} {3}", DateTime.UtcNow, type, tag, message));
            }
        }

        private void WriteLogMessage(string tag, string type, Throwable exc, string message)
        {
            using (var s = File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2} {3} {4}", DateTime.UtcNow, type, tag, message, exc));
            }
        }

        private void WriteLogMessage(string tag, string type ,string format, params object[] args)
        {
            using (var s = File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2} {3}", DateTime.UtcNow, type , tag, string.Format(format, args)));    
            }
        }

    }

    public static class LogMessageType
    {
        public const string Debug = "Debug";
        public const string Error = "Error";
        public const string Fatal = "Fatal";
        public const string Info = "Info";
        public const string Warning = "Warning";

    }
}