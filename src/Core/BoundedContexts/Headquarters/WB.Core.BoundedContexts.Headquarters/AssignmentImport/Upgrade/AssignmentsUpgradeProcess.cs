using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public class AssignmentsUpgradeProcess
    {
        public AssignmentsUpgradeProcess(Guid processId, Guid userId, QuestionnaireIdentity @from, QuestionnaireIdentity to)
        {
            ProcessId = processId;
            UserId = userId;
            From = @from;
            To = to;
        }
        public Guid ProcessId { get; }
        public Guid UserId { get; }
        public QuestionnaireIdentity From { get; }
        public QuestionnaireIdentity To { get; }
    }
}
