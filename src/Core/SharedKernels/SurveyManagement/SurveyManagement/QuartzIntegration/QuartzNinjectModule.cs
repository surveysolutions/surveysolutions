using Ninject;
using Ninject.Modules;
using Quartz;

namespace WB.Core.SharedKernels.SurveyManagement.QuartzIntegration
{
    public class QuartzNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISchedulerFactory>().To<NinjectSchedulerFactory>();
            Bind<IScheduler>().ToMethod(ctx => ctx.Kernel.Get<ISchedulerFactory>().GetScheduler()).InSingletonScope();
        }
    }
}