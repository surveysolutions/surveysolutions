using Ninject;
using Ninject.Modules;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class QuartzNinjectModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISchedulerFactory>().To<NinjectSchedulerFactory>();
            this.Bind<IScheduler>().ToMethod(ctx => ctx.Kernel.Get<ISchedulerFactory>().GetScheduler()).InSingletonScope();
        }
    }
}