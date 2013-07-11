using System;
using NLog;
using WB.Core.GenericSubdomains.Logging;

namespace WB.Core.SharedKernel.Utils.Logging
{
    public class NLogLogger : ILogger
    {
        private readonly global::NLog.Logger _log;

        private static readonly Type declaringType = typeof(NLogLogger);

        public NLogLogger(Type type)
        {
            _log = global::NLog.LogManager.GetLogger("logger", type);
        }

        public NLogLogger(global::NLog.Logger log)
        {
            if (log == null) throw new ArgumentNullException("log");

            _log = log;
        }

        public void Debug(string message, Exception exception = null)
        {
            if (!this.IsDebugEnabled) return;
            if (exception == null) 
                this.WriteInternal(LogLevel.Debug, message, null);

            this.WriteInternal(LogLevel.Debug, message, exception);
        }

        public void Info(string message, Exception exception = null)
        {
            if (!this.IsInfoEnabled) return;
            if (exception == null) 
                this.WriteInternal(LogLevel.Info, message, null);

            this.WriteInternal(LogLevel.Info, message, exception);
        }


        public void Warn(string message, Exception exception = null)
        {
            if (!this.IsWarnEnabled) return;
            if (exception == null) 
                this.WriteInternal(LogLevel.Warn, message, null);

            WriteInternal(LogLevel.Warn, message, exception);
        }

        public void Error(string message, Exception exception = null)
        {
            if (!this.IsErrorEnabled) return;
            if (exception == null) 
                this.WriteInternal(LogLevel.Error, message, null);

            this.WriteInternal(LogLevel.Error, message, exception);
        }

        public void Fatal(string message, Exception exception = null)
        {
            if (!this.IsFatalEnabled) return;
            if (exception == null) 
                this.WriteInternal(LogLevel.Fatal, message, null);
            this.WriteInternal(LogLevel.Fatal, message, exception);
        }

        internal bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }

        internal bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled; }
        }

        internal bool IsWarnEnabled
        {
            get { return _log.IsWarnEnabled; }
        }

        internal bool IsErrorEnabled
        {
            get { return _log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _log.IsFatalEnabled; }
        }

        protected void WriteInternal(LogLevel logLevel, object message, Exception exception)
        {
            var logEvent = new LogEventInfo(logLevel, _log.Name, null, "{0}", new object[] { message }, exception);
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
