using Quartz;
using Quartz.Impl;

namespace WB.Core.SharedKernels.SurveyManagement.QuartzIntegration
{
    public class NinjectSchedulerFactory : StdSchedulerFactory
    {
        private readonly NinjectJobFactory ninjectJobFactory;

        public NinjectSchedulerFactory(NinjectJobFactory ninjectJobFactory)
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