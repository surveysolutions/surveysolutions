using System;
using Java.Lang;
using WB.Common.Core.Logging;
using Exception = System.Exception;

namespace WB.Common
{

    public class FileLogger : ILog
    {
        private static readonly string BaseDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        private static readonly string LogFilename = System.IO.Path.Combine(BaseDir, "WBCapi.log.txt");
        private const string Tag = "Android.WBCapi";

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
                WriteLogMessage(Tag, message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
                WriteLogMessage(Tag, exception.ToThrowable(), message.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
                WriteLogMessage(Tag, format, args);
        }

        public void Info(object message)
        {
            if (IsInfoEnabled)
                WriteLogMessage(Tag, message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled)
                WriteLogMessage(Tag, exception.ToThrowable(), message.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
                WriteLogMessage(Tag, format, args);
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled)
                WriteLogMessage(Tag, message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
                WriteLogMessage(Tag, exception.ToThrowable(), message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
                WriteLogMessage(Tag, format, args);
        }

        public void Error(object message)
        {
            WriteLogMessage(Tag, message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
                WriteLogMessage(Tag, exception.ToThrowable(), message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
                WriteLogMessage(Tag, format, args);
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled)
                WriteLogMessage(Tag, message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
                WriteLogMessage(Tag, exception.ToThrowable(), message.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
                WriteLogMessage(Tag, format, args);
        }

        public bool IsDebugEnabled { get; private set; }
        public bool IsInfoEnabled { get; private set; }
        public bool IsWarnEnabled { get; private set; }
        public bool IsErrorEnabled { get; private set; }
        public bool IsFatalEnabled { get; private set; }

        private void WriteLogMessage(string tag, string message)
        {
            using (System.IO.StreamWriter s = System.IO.File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2}", DateTime.Now, tag, message));
            }

        }

        private void WriteLogMessage(string tag, Throwable exc, string message)
        {
            using (System.IO.StreamWriter s = System.IO.File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2} {3}", DateTime.Now, tag, message, exc.ToString()));
            }

        }

        private void WriteLogMessage(string tag, string format, params object[] args)
        {
            using (System.IO.StreamWriter s = System.IO.File.AppendText(LogFilename))
            {
                s.WriteLine(string.Format("{0} {1} {2}", DateTime.Now, tag, string.Format(format, args)));    
            }
        }

    }
}