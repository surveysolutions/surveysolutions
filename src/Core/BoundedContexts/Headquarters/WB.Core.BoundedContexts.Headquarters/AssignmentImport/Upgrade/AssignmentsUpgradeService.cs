using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public class QueuedUpgrade
    {
        public QueuedUpgrade(Guid processId, QuestionnaireIdentity @from, QuestionnaireIdentity to)
        {
            ProcessId = processId;
            From = @from;
            To = to;
        }
        public Guid ProcessId { get; }
        public QuestionnaireIdentity From { get; }
        public QuestionnaireIdentity To { get; }
    }

    internal class AssignmentsUpgradeService : IAssignmentsUpgradeService
    {
        private readonly Dictionary<Guid, AssignmentUpgradeProgressDetails> progressReporting = new Dictionary<Guid, AssignmentUpgradeProgressDetails>();
        private readonly ConcurrentQueue<QueuedUpgrade> upgradeQueue = new ConcurrentQueue<QueuedUpgrade>();

        public void EnqueueUpgrade(Guid processId, QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo)
        {
            this.upgradeQueue.Enqueue(new QueuedUpgrade(processId, migrateFrom, migrateTo));
            this.progressReporting[processId] = new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, 0, 0, new List<AssignmentUpgradeError>(), AssignmentUpgradeStatus.Queued);
        }

        public void ReportProgress(Guid processId, AssignmentUpgradeProgressDetails progressDetails)
        {
            this.progressReporting[processId] = progressDetails;
        }

        public QueuedUpgrade DequeueUpgrade()
        {
            if (!this.upgradeQueue.IsEmpty)
            {
                if(this.upgradeQueue.TryDequeue(out QueuedUpgrade request))
                {
                    return request;
                }
            }

            return null;
        }

        public AssignmentUpgradeProgressDetails Status(Guid processId)
        {
            if (progressReporting.ContainsKey(processId))
            {
                return progressReporting[processId];
            }

            return null;
        }
    }
}
