using System;
using NLog;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    internal class NLogLogger : ILogger
    {
        private readonly Logger logger;

        public NLogLogger(string type)
        {
            this.logger = LogManager.GetLogger(type);
        }

        public void Debug(string message, Exception exception = null)
        {
            this.logger.DebugException(message, exception);
        }

        public void Info(string message, Exception exception = null)
        {
            this.logger.InfoException(message, exception);
        }


        public void Warn(string message, Exception exception = null)
        {
            this.logger.WarnException(message, exception);
        }

        public void Error(string message, Exception exception = null)
        {
            this.logger.ErrorException(message, exception);
        }

        public void Fatal(string message, Exception exception = null)
        {
            this.logger.FatalException(message, exception);
        }
    }
}
