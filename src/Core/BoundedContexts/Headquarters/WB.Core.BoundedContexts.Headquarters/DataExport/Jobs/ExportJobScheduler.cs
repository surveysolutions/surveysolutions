using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs
{
    public class ExportJobScheduler
    {
        private readonly IScheduler scheduler;

        private readonly ExportSettings settings;

        public ExportJobScheduler(IScheduler scheduler, ExportSettings settings)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            this.scheduler = scheduler;
            this.settings = settings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<ExportJob>()
                .WithIdentity("export job", "Export")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("export trigger", "Export")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(this.settings.BackgroundExportIntervalInSeconds)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}