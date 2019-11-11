using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Modularity;

namespace WB.Infrastructure.Native.Logging
{
    public class NLogLoggingModule : IModule, IAppModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ILoggerProvider, NLogLoggerProvider>();

            registry.BindToMethod<ILogger>(context =>
            {
                if (context.MemberDeclaringType != null)
                    return new NLogLogger(context.MemberDeclaringType);

                return new NLogLogger("UNKNOWN");
            });
        }

        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<ILoggerProvider, NLogLoggerProvider>();
            registry.Bind<ILogger, NLogLogger>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status) => Task.CompletedTask;
        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status) => Task.CompletedTask;
    }
}
