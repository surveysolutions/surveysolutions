using System.Threading.Tasks;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class QuartzModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ISchedulerFactory, NinjectSchedulerFactory>();
            registry.BindToMethodInSingletonScope<IScheduler>(ctx => ctx.Get<ISchedulerFactory>().GetScheduler());
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
