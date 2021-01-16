using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public static class QuartzRegistrationHelpers
    {
        public static void BindScheduledJob<TJ, TD>(this IIocRegistry registry)
            where TJ: IJob<TD>
        {
            registry.Bind<TJ>();
            registry.Bind<IScheduledTask<TJ, TD>, ScheduledTask<TJ, TD>>();
            registry.Bind<IScheduledJob, ScheduledTask<TJ, TD>>();
        }
    }
}
