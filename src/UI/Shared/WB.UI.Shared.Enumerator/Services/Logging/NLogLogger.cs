using System;
using MvvmCross.Platform;
using MvvmCross.Platform.Exceptions;
using MvvmCross.Platform.Platform;
using NLog;
using Exception = System.Exception;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Shared.Enumerator.Services.Logging
{
    public class NLogLogger : ILogger
    {
        private readonly Logger logger;

        public NLogLogger()
            : this(string.Empty) { }

        public NLogLogger(Type type)
            : this(type.FullName) { }

        public NLogLogger(string name)
        {
            this.logger = LogManager.GetLogger(name);
        }

        public void Trace(string message, Exception exception = null)
        {
            this.logger.Trace(exception, message);
        }

        public void Debug(string message, Exception exception = null)
        {
            Mvx.Trace(MvxTraceLevel.Diagnostic, message);

            this.logger.Debug(exception, message);
        }

        public void Info(string message, Exception exception = null)
        {
            Mvx.Trace(MvxTraceLevel.Diagnostic, message);

            this.logger.Info(exception, message);
        }

        public void Warn(string message, Exception exception = null)
        {
            Mvx.Warning(message);

            this.logger.Warn(exception, message);
        }

        public void Error(string message, Exception exception = null)
        {
            Mvx.Error(message + " " + exception?.ToLongString());

            this.logger.Error(exception, message);
        }

        public void Fatal(string message, Exception exception = null)
        {
            Mvx.Error(message + " " + exception?.ToLongString());

            this.logger.Fatal(exception, message);
        }
    }
}
