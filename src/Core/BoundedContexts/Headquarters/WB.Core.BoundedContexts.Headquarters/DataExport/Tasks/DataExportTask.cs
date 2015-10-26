using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Tasks
{
    public class DataExportTask
    {
        private readonly IScheduler scheduler;
        private readonly DataExportSettings dataExportSettings;

        public DataExportTask(IScheduler scheduler, DataExportSettings dataExportSettings)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            this.scheduler = scheduler;
            this.dataExportSettings = dataExportSettings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<DataExportJob>()
                .WithIdentity("data export", "Data export")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("data export trigger", "Data export")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(dataExportSettings.DataExportIntervalInSeconds)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}