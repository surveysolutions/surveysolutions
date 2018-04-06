using System;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    [DisallowConcurrentExecution]
    public class UpgradeAssignmentJob : IJob
    {
        private readonly IAssignmentsUpgradeService upgradeService =
            ServiceLocator.Current.GetInstance<IAssignmentsUpgradeService>();

        public Task Execute(IJobExecutionContext context)
        {
            var processToRun = upgradeService.DequeueUpgrade();
            if (processToRun != null)
            {
                return Task.Run(() =>
                {
                    ServiceLocator.Current.GetInstance<IAssignmentsUpgrader>().Upgrade(processToRun.From, processToRun.To);
                });
            }

            return Task.CompletedTask;
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

        public async Task ConfigureAsync()
        {
            IJobDetail job = JobBuilder.Create<ExportJob>()
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
