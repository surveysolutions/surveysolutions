using System;
using System.IO;
using Java.Lang;
using WB.Core.SharedKernel.Logger;
using Environment = Android.OS.Environment;
using Exception = System.Exception;

namespace WB.Core.SharedKernel.Utils.Logging
{

    public class FileLogger : ILog
    {

        private static readonly string LogFilename = System.IO.Path.Combine(GetLogDirectory(), "WBCapi.log.txt");
        
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

        public void Debug(object message)
        {
            if (IsDebugEnabled)
                WriteLogMessage(Tag, LogMessageType.Debug, message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
                WriteLogMessage(Tag, LogMessageType.Debug, exception.ToThrowable(), message.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
                WriteLogMessage(Tag, LogMessageType.Debug, format, args);
        }

        public void Info(object message)
        {
            if (IsInfoEnabled)
                WriteLogMessage(Tag, LogMessageType.Info, message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled)
                WriteLogMessage(Tag, LogMessageType.Info, exception.ToThrowable(), message.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
                WriteLogMessage(Tag, LogMessageType.Info, format, args);
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled)
                WriteLogMessage(Tag, LogMessageType.Warning, message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
                WriteLogMessage(Tag, LogMessageType.Warning, exception.ToThrowable(), message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
                WriteLogMessage(Tag, LogMessageType.Warning, format, args);
        }

        public void Error(object message)
        {
            WriteLogMessage(Tag, LogMessageType.Error, message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
                WriteLogMessage(Tag, LogMessageType.Error, exception.ToThrowable(), message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
                WriteLogMessage(Tag, LogMessageType.Error, format, args);
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled)
                WriteLogMessage(Tag, LogMessageType.Fatal, message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
                WriteLogMessage(Tag, LogMessageType.Fatal, exception.ToThrowable(), message.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
                WriteLogMessage(Tag, LogMessageType.Fatal, format, args);
        }

        public bool IsDebugEnabled { get; private set; }
        public bool IsInfoEnabled { get; private set; }
        public bool IsWarnEnabled { get; private set; }
        public bool IsErrorEnabled { get; private set; }
        public bool IsFatalEnabled { get; private set; }

        private void WriteLogMessage(string tag, string type,  string message)
        {
            using (System.IO.StreamWriter s = System.IO.File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2} {3}", DateTime.UtcNow, type, tag, message));
            }
        }

        private void WriteLogMessage(string tag, string type, Throwable exc, string message)
        {
            using (System.IO.StreamWriter s = System.IO.File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2} {3} {4}", DateTime.UtcNow, type, tag, message, exc.ToString()));
            }
        }

        private void WriteLogMessage(string tag, string type ,string format, params object[] args)
        {
            using (System.IO.StreamWriter s = System.IO.File.AppendText(LogFilename))
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