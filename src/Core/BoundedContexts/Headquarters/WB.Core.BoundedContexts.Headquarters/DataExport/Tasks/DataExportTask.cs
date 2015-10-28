using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Tasks
{
    public class DataExportTask
    {
        const string DataExportJobName = "data export";
        const string DataExportJobGroup = "DataExport";
        private readonly IScheduler scheduler;
        private IJobDetail job;

        public DataExportTask(IScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            this.scheduler = scheduler;
        }

        public void Configure()
        {
            job = JobBuilder.Create<DataExportJob>()
                .WithIdentity(DataExportJobName, DataExportJobGroup)
                .StoreDurably(true)
                .Build();
            
            this.scheduler.AddJob(job, true);
        }

        public void TriggerJob()
        {
            this.scheduler.TriggerJob(new JobKey(DataExportJobName, DataExportJobGroup));
        }
    }
}