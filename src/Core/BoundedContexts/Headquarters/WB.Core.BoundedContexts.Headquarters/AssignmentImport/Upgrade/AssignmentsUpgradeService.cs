using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    internal class AssignmentsUpgradeService : IAssignmentsUpgradeService
    {
        private readonly ISystemLog auditLog;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IUserRepository users;
        private readonly IScheduledTask<UpgradeAssignmentJob, QueuedUpgrade> scheduler;
        private static readonly Dictionary<Guid, AssignmentUpgradeProgressDetails> progressReporting = new();
        private readonly Dictionary<Guid, CancellationTokenSource> cancellationTokens = new();

        public AssignmentsUpgradeService(ISystemLog auditLog, 
            IQuestionnaireStorage questionnaireStorage,
            IUserRepository users,
            IScheduledTask<UpgradeAssignmentJob, QueuedUpgrade> scheduler)
        {
            this.auditLog = auditLog;
            this.questionnaireStorage = questionnaireStorage;
            this.users = users;
            this.scheduler = scheduler;
        }

        public void EnqueueUpgrade(Guid processId,
            Guid userId,
            QuestionnaireIdentity migrateFrom,
            QuestionnaireIdentity migrateTo)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null);

            var user = this.users.FindById(userId);
            this.auditLog.AssignmentsUpgradeStarted(questionnaire.Title, migrateFrom.Version, migrateTo.Version, userId, user.UserName);

            var upgrade = new QueuedUpgrade(processId, userId, migrateFrom, migrateTo);
            
            progressReporting[processId] = new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, 0, 0, new List<AssignmentUpgradeError>(), AssignmentUpgradeStatus.Queued);
            scheduler.Schedule(upgrade).GetAwaiter().GetResult();
        }

        public void ReportProgress(Guid processId, AssignmentUpgradeProgressDetails progressDetails)
        {
            progressReporting[processId] = progressDetails;
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
            var cancellationTokenSource = this.cancellationTokens.GetOrAdd(processId, () => new CancellationTokenSource());
            return cancellationTokenSource.Token;
        }

        public void StopProcess(Guid processId)
        {
            if (this.cancellationTokens.ContainsKey(processId))
            {
                this.cancellationTokens[processId].Cancel();
            }
        }
    }
}
