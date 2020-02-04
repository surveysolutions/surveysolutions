using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.LoggingIntegration
{
    public class SerilogLoggerModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ILoggerProvider, SerilogLoggerProvider>();
            registry.Bind<Microsoft.Extensions.Logging.ILoggerProvider, Serilog.Extensions.Logging.SerilogLoggerProvider>();

            registry.BindToMethod<ILogger>(context =>
            {
                if (context.MemberDeclaringType != null)
                    return new SerilogLogger(context.MemberDeclaringType);

                return new SerilogLogger();
            });
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
