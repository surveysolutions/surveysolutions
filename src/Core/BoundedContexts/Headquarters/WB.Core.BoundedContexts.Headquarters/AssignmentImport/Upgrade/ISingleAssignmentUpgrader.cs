using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public interface ISingleAssignmentUpgrader
    {
        void Upgrade(int assignmentId, IQuestionnaire targetQuestionnaire, Guid userId,
            QuestionnaireIdentity migrateTo);
    }
}