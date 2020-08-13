
using WB.Core.Infrastructure.Modularity;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using ILoggerProvider = WB.Core.GenericSubdomains.Portable.Services.ILoggerProvider;

namespace WB.UI.Shared.Web.LoggingIntegration
{
    public class SerilogLoggerModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ILoggerProvider, SerilogLoggerProvider>();
            registry.Bind<Microsoft.Extensions.Logging.ILoggerProvider, Serilog.Extensions.Logging.SerilogLoggerProvider>();

            registry.Bind<ILogger, SerilogLogger>();
        }
    }
}
