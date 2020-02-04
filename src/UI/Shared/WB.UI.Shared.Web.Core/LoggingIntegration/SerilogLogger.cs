using System;
using Serilog;

namespace WB.UI.Shared.Web.LoggingIntegration
{
    public class SerilogLogger : Core.GenericSubdomains.Portable.Services.ILogger
    {
        private readonly Serilog.ILogger logger;

        public SerilogLogger()
        {
            this.logger = Log.Logger;
        }

        public SerilogLogger(Type type)
        {
            this.logger = Log.ForContext(type);
        }

        public void Trace(string message, Exception exception = null)
        {
            this.logger.Verbose(exception, message);
        }

        public void Debug(string message, Exception exception = null)
        {
            this.logger.Debug(exception, message);
        }

        public void Info(string message, Exception exception = null)
        {
            this.logger.Information(exception, message);
        }

        public void Warn(string message, Exception exception = null)
        {
            this.logger.Warning(exception, message);
        }

        public void Error(string message, Exception exception = null)
        {
            this.logger.Error(exception, message);
        }

        public void Fatal(string message, Exception exception = null)
        {
            this.logger.Fatal(exception, message);
        }
    }

    public class SerilogLoggerProvider : WB.Core.GenericSubdomains.Portable.Services.ILoggerProvider
    {
        public Core.GenericSubdomains.Portable.Services.ILogger GetFor<T>()
        {
            return new SerilogLogger(typeof(T));
        }

        public Core.GenericSubdomains.Portable.Services.ILogger GetForType(Type type)
        {
            return new SerilogLogger(type);
        }
    }
}
