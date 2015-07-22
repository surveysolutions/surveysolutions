using System;
using EventStore.ClientAPI;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    public class EventStoreLogger : ILogger
    {
        private readonly GenericSubdomains.Portable.Services.ILogger genericLogger;

        public EventStoreLogger(GenericSubdomains.Portable.Services.ILogger genericLogger)
        {
            this.genericLogger = genericLogger;
        }

        public void Error(string format, params object[] args)
        {
            this.genericLogger.Error(string.Format(format, args));
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            this.genericLogger.Error(string.Format(format, args), ex);
        }

        public void Info(string format, params object[] args)
        {
            this.genericLogger.Info(string.Format(format, args));
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            this.genericLogger.Info(string.Format(format, args), ex);
        }

        public void Debug(string format, params object[] args)
        {
            this.genericLogger.Debug(string.Format(format, args));
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            this.genericLogger.Debug(string.Format(format, args), ex);
        }
    }
}