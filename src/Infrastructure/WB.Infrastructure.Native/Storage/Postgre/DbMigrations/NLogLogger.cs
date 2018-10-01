using System;
using Microsoft.Extensions.Logging;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public class NLogLogger : ILogger
    {
        private readonly NLog.Logger logger;

        public NLogLogger(NLog.Logger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            return NLog.NestedDiagnosticsContext.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            NLog.LogLevel convertedLogLevel = GetNLogLogLevel(logLevel);

            return this.logger.IsEnabled(convertedLogLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            string message = formatter(state, exception);

            this.LogNLog(logLevel, message);
        }

        private void LogNLog(LogLevel logLevel, string message)
        {
            NLog.LogLevel convertedLogLevel = GetNLogLogLevel(logLevel);

            this.logger.Log(convertedLogLevel, message);
        }

        private static NLog.LogLevel GetNLogLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;

                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;

                case LogLevel.Information:
                    return NLog.LogLevel.Info;

                case LogLevel.Warning:
                    return NLog.LogLevel.Info;

                case LogLevel.Error:
                    return NLog.LogLevel.Error;

                case LogLevel.Critical:
                    return NLog.LogLevel.Fatal;

                case LogLevel.None:
                    return NLog.LogLevel.Off;

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}