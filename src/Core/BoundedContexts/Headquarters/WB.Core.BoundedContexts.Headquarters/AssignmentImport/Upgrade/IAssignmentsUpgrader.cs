using System.Threading;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public interface IAssignmentsUpgrader
    {
        void Upgrade(QueuedUpgrade queuedUpgrade, CancellationToken cancellationToken = default);
    }
}
