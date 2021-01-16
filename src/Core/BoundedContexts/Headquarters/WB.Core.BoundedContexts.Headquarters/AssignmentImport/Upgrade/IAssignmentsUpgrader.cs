using System.Threading;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public interface IAssignmentsUpgrader
    {
        void Upgrade(AssignmentsUpgradeProcess assignmentsUpgradeProcess, CancellationToken cancellationToken = default);
    }
}
