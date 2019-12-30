using System;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;

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

        public Task Execute(IJobExecutionContext context)
        {
            var processToRun = assignmentsUpgradeService.DequeueUpgrade();
            if (processToRun != null)
            {
                assignmentsUpgrader.Upgrade(processToRun.ProcessId, processToRun.UserId, processToRun.From, processToRun.To,
                    assignmentsUpgradeService.GetCancellationToken(processToRun.ProcessId));
            }

            return Task.CompletedTask;
        }
    }

    public class UpgradeAssignmentJobScheduler
    {
        private readonly IScheduler scheduler;

        private readonly AssignmentImportOptions settings;

        public UpgradeAssignmentJobScheduler(IScheduler scheduler, AssignmentImportOptions settings)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task Configure()
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

            await this.scheduler.ScheduleJob(job, trigger);

            await this.scheduler.AddJob(job, true);
        }
    }
}
