using System;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

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
                assignmentsUpgrader.Upgrade(processToRun.ProcessId, processToRun.UserId, processToRun.From,
                    processToRun.To,
                    assignmentsUpgradeService.GetCancellationToken(processToRun.ProcessId));
            }

            return Task.CompletedTask;
        }
    }

    public class UpgradeAssignmentJobScheduler : BaseTask
    {
        public UpgradeAssignmentJobScheduler(IScheduler scheduler) : base(scheduler, "Import",
            typeof(UpgradeAssignmentJob))
        {
        }
    }
}
