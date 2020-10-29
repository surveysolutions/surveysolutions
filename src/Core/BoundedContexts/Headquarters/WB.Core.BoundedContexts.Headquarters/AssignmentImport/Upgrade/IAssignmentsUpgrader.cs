using System;
using System.Threading;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public interface IAssignmentsUpgrader
    {
        void Upgrade(Guid processId, Guid userId, QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo,
            CancellationToken cancellationToken);
    }
}