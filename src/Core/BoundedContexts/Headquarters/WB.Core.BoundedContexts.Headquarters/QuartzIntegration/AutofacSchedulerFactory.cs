using Quartz;
using Quartz.Impl;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AutofacSchedulerFactory : StdSchedulerFactory
    {
        private readonly AutofacJobFactory autofacJobFactory;

        public AutofacSchedulerFactory(AutofacJobFactory autofacJobFactory, IQuartzSettings quartzSettings): base(quartzSettings.GetSettings())
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
