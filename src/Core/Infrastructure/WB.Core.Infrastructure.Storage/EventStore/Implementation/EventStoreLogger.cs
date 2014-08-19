using System;
using EventStore.ClientAPI;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    public class EventStoreLogger : ILogger
    {
        private readonly GenericSubdomains.Logging.ILogger capiLogger;

        public EventStoreLogger(GenericSubdomains.Logging.ILogger capiLogger)
        {
            this.capiLogger = capiLogger;
        }

        public void Error(string format, params object[] args)
        {
            capiLogger.Error(string.Format(format, args));
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            capiLogger.Error(string.Format(format, args), ex);
        }

        public void Info(string format, params object[] args)
        {
            capiLogger.Info(string.Format(format, args));
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            capiLogger.Info(string.Format(format, args), ex);
        }

        public void Debug(string format, params object[] args)
        {
            capiLogger.Debug(string.Format(format, args));
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            capiLogger.Debug(string.Format(format, args), ex);
        }
    }
}