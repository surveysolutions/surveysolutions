using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    internal class AssignmentsUpgradeService : IAssignmentsUpgradeService
    {
        private readonly ISystemLog auditLog;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IUserRepository users;
        private static readonly Dictionary<Guid, AssignmentUpgradeProgressDetails> progressReporting = new Dictionary<Guid, AssignmentUpgradeProgressDetails>();
        private static readonly ConcurrentQueue<QueuedUpgrade> upgradeQueue = new ConcurrentQueue<QueuedUpgrade>();
        private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> cancellationTokens = new ConcurrentDictionary<Guid, CancellationTokenSource>();

        public AssignmentsUpgradeService(ISystemLog auditLog, 
            IQuestionnaireStorage questionnaireStorage,
            IUserRepository users)
        {
            this.auditLog = auditLog;
            this.questionnaireStorage = questionnaireStorage;
            this.users = users;
        }

        public void EnqueueUpgrade(Guid processId,
            Guid userId,
            QuestionnaireIdentity migrateFrom,
            QuestionnaireIdentity migrateTo)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null);

            var user = this.users.FindById(userId);
            this.auditLog.AssignmentsUpgradeStarted(questionnaire.Title, migrateFrom.Version, migrateTo.Version, userId, user.UserName);

            upgradeQueue.Enqueue(new QueuedUpgrade(processId, userId, migrateFrom, migrateTo));
            progressReporting[processId] = new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, 0, 0, new List<AssignmentUpgradeError>(), AssignmentUpgradeStatus.Queued);
        }

        public void ReportProgress(Guid processId, AssignmentUpgradeProgressDetails progressDetails)
        {
            progressReporting[processId] = progressDetails;
        }

        public QueuedUpgrade DequeueUpgrade()
        {
            if (!upgradeQueue.IsEmpty)
            {
                if(upgradeQueue.TryDequeue(out QueuedUpgrade request))
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

        public CancellationToken GetCancellationToken(Guid processId)
        {
            var cancellationTokenSource = cancellationTokens.GetOrAdd(processId, (processId) => new CancellationTokenSource());
            return cancellationTokenSource.Token;
        }

        public void StopProcess(Guid processId)
        {
            if (cancellationTokens.ContainsKey(processId))
            {
                cancellationTokens[processId].Cancel();
            }
        }
    }
}
