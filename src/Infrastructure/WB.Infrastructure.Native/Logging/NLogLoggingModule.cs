using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity;

namespace WB.Infrastructure.Native.Logging
{
    public class NLogLoggingModule : IModule
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

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
