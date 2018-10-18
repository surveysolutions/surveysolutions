using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    [DisallowConcurrentExecution]
    public class UpgradeAssignmentJob : IJob
    {
        public UpgradeAssignmentJob(IAssignmentsUpgradeService assignmentsUpgradeService, 
            IAssignmentsUpgrader assignmentsUpgrader)
        {
            this.assignmentsUpgradeService = assignmentsUpgradeService;
            this.assignmentsUpgrader = assignmentsUpgrader;
        }
        
        private readonly IAssignmentsUpgradeService assignmentsUpgradeService;
        private readonly IAssignmentsUpgrader assignmentsUpgrader;

        public void Execute(IJobExecutionContext context)
        {
            var processToRun = assignmentsUpgradeService.DequeueUpgrade();
            if (processToRun != null)
            {
                assignmentsUpgrader.Upgrade(processToRun.ProcessId, processToRun.From, processToRun.To,
                    assignmentsUpgradeService.GetCancellationToken(processToRun.ProcessId));
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
