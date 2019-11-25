using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AutofacSchedulerFactory : StdSchedulerFactory
    {
        private readonly IJobFactory autofacJobFactory;

        public AutofacSchedulerFactory(IJobFactory autofacJobFactory, IQuartzSettings quartzSettings): base(quartzSettings.GetSettings())
        {
            this.autofacJobFactory = autofacJobFactory;
        }

        protected override IScheduler Instantiate(Quartz.Core.QuartzSchedulerResources rsrcs, Quartz.Core.QuartzScheduler qs)
        {
            qs.JobFactory = this.autofacJobFactory;

            return base.Instantiate(rsrcs, qs);
        }
    }
}
