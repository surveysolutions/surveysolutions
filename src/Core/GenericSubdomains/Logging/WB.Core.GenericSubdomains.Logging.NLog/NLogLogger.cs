using System;
using NLog;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    internal class NLogLogger : ILogger
    {
        private readonly global::NLog.Logger logger;

        public NLogLogger(global::NLog.Logger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        public void Debug(string message, Exception exception = null)
        {
            if (!this.IsDebugEnabled) return;
            if (exception == null)
            {
                this.logger.Debug(new LogEventInfo(LogLevel.Debug, this.logger.Name, null, "{0}", new[] { (object) message }, null));
            }

            this.logger.Debug(new LogEventInfo(LogLevel.Debug, this.logger.Name, null, "{0}", new[] { (object) message }, exception));
        }

        public void Info(string message, Exception exception = null)
        {
            if (!this.IsInfoEnabled) return;
            if (exception == null)
            {
                this.logger.Info(new LogEventInfo(LogLevel.Info, this.logger.Name, null, "{0}", new[] { (object)message }, null));
            }

            this.logger.Info(new LogEventInfo(LogLevel.Info, this.logger.Name, null, "{0}", new[] { (object)message }, exception));
        }


        public void Warn(string message, Exception exception = null)
        {
            if (!this.IsWarnEnabled) return;
            if (exception == null)
            {
                this.logger.Warn(new LogEventInfo(LogLevel.Warn, this.logger.Name, null, "{0}", new[] { (object)message }, null));
            }

            this.logger.Warn(new LogEventInfo(LogLevel.Warn, this.logger.Name, null, "{0}", new[] { (object)message }, exception));
        }

        public void Error(string message, Exception exception = null)
        {
            if (!this.IsErrorEnabled) return;
            if (exception == null)
            {
                this.logger.Error(new LogEventInfo(LogLevel.Error, this.logger.Name, null, "{0}", new[] { (object)message }, null));
            }

            this.logger.Error(new LogEventInfo(LogLevel.Error, this.logger.Name, null, "{0}", new[] { (object)message }, exception));
        }

        public void Fatal(string message, Exception exception = null)
        {
            if (!this.IsFatalEnabled) return;
            if (exception == null)
            {
                this.logger.Fatal(new LogEventInfo(LogLevel.Fatal, this.logger.Name, null, "{0}", new[] { (object)message }, null));
            }
            this.logger.Fatal(new LogEventInfo(LogLevel.Fatal, this.logger.Name, null, "{0}", new[] { (object)message }, exception));
        }

        internal bool IsDebugEnabled
        {
            get { return this.logger.IsDebugEnabled; }
        }

        internal bool IsInfoEnabled
        {
            get { return this.logger.IsInfoEnabled; }
        }

        internal bool IsWarnEnabled
        {
            get { return this.logger.IsWarnEnabled; }
        }

        internal bool IsErrorEnabled
        {
            get { return this.logger.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return this.logger.IsFatalEnabled; }
        }
    }
}
