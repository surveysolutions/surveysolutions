using System;
using NLog;
using WB.Core.SharedKernel.Logger;

namespace WB.Core.SharedKernel.Utils.Logging
{
    public class NLogLogger : ILog
    {
        private readonly global::NLog.Logger _log;

        private static readonly Type declaringType = typeof (NLogLogger);

        public NLogLogger(Type type)
        {
            _log = global::NLog.LogManager.GetLogger("logger", type);
        }

        public NLogLogger(global::NLog.Logger log)
        {
            if (log == null) throw new ArgumentNullException("log");

            _log = log;
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled)
                WriteInternal(LogLevel.Debug, message, null);

        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
                WriteInternal(LogLevel.Debug, message, exception);

        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
                WriteInternal(LogLevel.Debug, new StringFormatFormattedMessage(null, format, args), null);
        }

        public void Info(object message)
        {
            if (IsInfoEnabled)
                WriteInternal(LogLevel.Info, message, null);
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled)
                WriteInternal(LogLevel.Info, message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
                WriteInternal(LogLevel.Info, new StringFormatFormattedMessage(null, format, args), null);
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled)
                WriteInternal(LogLevel.Warn, message, null);
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
                WriteInternal(LogLevel.Warn, message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
                WriteInternal(LogLevel.Warn, new StringFormatFormattedMessage(null, format, args), null);
        }

        public void Error(object message)
        {
            if (IsErrorEnabled)
                WriteInternal(LogLevel.Error, message, null);
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
                WriteInternal(LogLevel.Error, message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
                WriteInternal(LogLevel.Error, new StringFormatFormattedMessage(null, format, args), null);
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled)
                WriteInternal(LogLevel.Fatal, message, null);
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
                WriteInternal(LogLevel.Fatal, message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
                WriteInternal(LogLevel.Fatal, new StringFormatFormattedMessage(null, format, args), null);
        }

        public bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _log.IsWarnEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _log.IsFatalEnabled; }
        }


        protected void WriteInternal(LogLevel logLevel, object message, Exception exception)
        {
            LogEventInfo logEvent = new LogEventInfo(logLevel, _log.Name, null, "{0}", new object[] {message}, exception);

            _log.Log(declaringType, logEvent);
        }
        
        protected class StringFormatFormattedMessage
        {
            private volatile string cachedMessage;

            private readonly IFormatProvider FormatProvider;
            private readonly string Message;
            private readonly object[] Args;

            
            public StringFormatFormattedMessage(IFormatProvider formatProvider, string message, params object[] args)
            {
                FormatProvider = formatProvider;
                Message = message;
                Args = args;
            }

            public override string ToString()
            {
                if (cachedMessage == null && Message != null)
                {
                    cachedMessage = string.Format(FormatProvider, Message, Args);
                }
                return cachedMessage;
            }
        }
    }
}
