using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs
{
    public class ExportDataRetensionTask : BaseTask
    {
        public ExportDataRetensionTask(ISchedulerFactory schedulerFactory) 
            : base(schedulerFactory, "Export Files Retention", typeof(ExportDataRetentionJob)) { }
    }
}
