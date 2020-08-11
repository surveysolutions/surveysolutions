using System;
using Serilog;

namespace WB.UI.Shared.Web.LoggingIntegration
{
    public class SerilogLoggerProvider : WB.Core.GenericSubdomains.Portable.Services.ILoggerProvider
    {
        private readonly ILogger logger;

        public SerilogLoggerProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public Core.GenericSubdomains.Portable.Services.ILogger GetFor<T>()
        {
            return new SerilogLogger(logger.ForContext<T>());
        }

        public Core.GenericSubdomains.Portable.Services.ILogger GetForType(Type type)
        {
            return new SerilogLogger(logger);
        }
    }
}
