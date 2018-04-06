using System.Collections.Concurrent;
using System.Windows.Media.Animation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public class QueuedUpgrade
    {
        public QueuedUpgrade(QuestionnaireIdentity @from, QuestionnaireIdentity to)
        {
            From = @from;
            To = to;
        }

        public QuestionnaireIdentity From { get; }
        public QuestionnaireIdentity To { get; }
    }

    internal class AssignmentsUpgradeService : IAssignmentsUpgradeService
    {
        private readonly ConcurrentQueue<QueuedUpgrade> upgradeQueue = new ConcurrentQueue<QueuedUpgrade>();

        private AssignmentUpgradeProgressDetails currentProggress;

        public void EnqueueUpgrade(QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo)
        {
            this.upgradeQueue.Enqueue(new QueuedUpgrade(migrateFrom, migrateTo));
        }

        public void ReportProgress(QuestionnaireIdentity targetQuestionnaire, AssignmentUpgradeProgressDetails progressDetails)
        {
            this.currentProggress = progressDetails;
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
    }
}
