using System;

using NLog;

using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Native.Logging
{
    internal class NLogLogger : ILogger
    {
        private readonly Logger logger;

        public NLogLogger()
        {
            this.logger = LogManager.GetLogger("");
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
