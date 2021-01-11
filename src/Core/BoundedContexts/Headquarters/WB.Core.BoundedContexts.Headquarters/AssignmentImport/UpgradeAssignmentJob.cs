
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    [DisallowConcurrentExecution]
    [RetryFailedJob]
    [DisplayName("Upgrade assignments"), Category("Import")]
    public class UpgradeAssignmentJob : IJob<AssignmentsUpgradeProcess>
    {
        public UpgradeAssignmentJob(IAssignmentsUpgrader assignmentsUpgrader)
        {
            this.assignmentsUpgrader = assignmentsUpgrader;
        }

        private readonly IAssignmentsUpgrader assignmentsUpgrader;

        public Task Execute(AssignmentsUpgradeProcess data, IJobExecutionContext context)
        {
            assignmentsUpgrader.Upgrade(data, CancellationToken.None);

            return Task.CompletedTask;
        }
    }
}
