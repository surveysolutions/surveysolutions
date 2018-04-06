using System.Threading.Tasks;
using Quartz;
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
            var processToRun = upgradeService.GetProcessToRun();
            if (processToRun != null)
            {
                return Task.Run(() =>
                {
                    ServiceLocator.Current.GetInstance<IAssignmentsUpgrader>().Upgrade(processToRun.MigrateFrom, processToRun.MigrateTo);
                });
            }

            return Task.CompletedTask;
        }
    }
}
