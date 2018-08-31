using Quartz;
using Quartz.Impl;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AutofacSchedulerFactory : StdSchedulerFactory
    {
        private readonly AutofacJobFactory ninjectJobFactory;

        public AutofacSchedulerFactory(AutofacJobFactory ninjectJobFactory)
        {
            this.ninjectJobFactory = ninjectJobFactory;
        }

        protected override IScheduler Instantiate(Quartz.Core.QuartzSchedulerResources rsrcs, Quartz.Core.QuartzScheduler qs)
        {
            qs.JobFactory = this.ninjectJobFactory;
            return base.Instantiate(rsrcs, qs);
        }
    }
}
