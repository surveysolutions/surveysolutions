using Quartz;
using Quartz.Impl;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AutofacSchedulerFactory : StdSchedulerFactory
    {
        private readonly AutofacJobFactory autifacJobFactory;

        public AutofacSchedulerFactory(AutofacJobFactory autifacJobFactory)
        {
            this.autifacJobFactory = autifacJobFactory;
        }

        protected override IScheduler Instantiate(Quartz.Core.QuartzSchedulerResources rsrcs, Quartz.Core.QuartzScheduler qs)
        {
            qs.JobFactory = this.autifacJobFactory;
            return base.Instantiate(rsrcs, qs);
        }
    }
}
