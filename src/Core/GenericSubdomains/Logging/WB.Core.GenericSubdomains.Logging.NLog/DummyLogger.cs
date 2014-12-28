using System;
using NLog;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    internal class DummyLogger : ILogger
    {
        public DummyLogger()
        {}

        public void Debug(string message, Exception exception = null)
        {}

        public void Info(string message, Exception exception = null)
        {}


        public void Warn(string message, Exception exception = null)
        {}

        public void Error(string message, Exception exception = null)
        {}

        public void Fatal(string message, Exception exception = null)
        {}
    }
}
