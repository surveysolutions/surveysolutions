using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    [DisallowConcurrentExecution]
    public class UpgradeAssignmentJob : IJob
    {
        private readonly IAssignmentsUpgradeService upgradeService =
            ServiceLocator.Current.GetInstance<IAssignmentsUpgradeService>();

        public void Execute(IJobExecutionContext context)
        {
            var processToRun = upgradeService.DequeueUpgrade();
            if (processToRun != null)
            {
                ServiceLocator.Current.GetInstance<IAssignmentsUpgrader>().Upgrade(processToRun.ProcessId, processToRun.From, processToRun.To, upgradeService.GetCancellationToken(processToRun.ProcessId));
            }
        }
    }

    public class UpgradeAssignmentJobScheduler
    {
        private readonly IScheduler scheduler;

        private readonly ExportSettings settings;

        public UpgradeAssignmentJobScheduler(IScheduler scheduler, ExportSettings settings)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<UpgradeAssignmentJob>()
                .WithIdentity("assignment upgrade job", "Import")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("assignment upgrade", "Import")
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
