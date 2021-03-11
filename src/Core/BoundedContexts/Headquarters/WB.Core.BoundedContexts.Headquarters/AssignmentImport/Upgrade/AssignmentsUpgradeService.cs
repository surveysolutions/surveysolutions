using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache memoryCache;
        
        private static readonly Dictionary<Guid, AssignmentUpgradeProgressDetails> progressReporting = new Dictionary<Guid, AssignmentUpgradeProgressDetails>();
        private static readonly ConcurrentQueue<QueuedUpgrade> upgradeQueue = new ConcurrentQueue<QueuedUpgrade>();

        public AssignmentsUpgradeService(ISystemLog auditLog, 
            IQuestionnaireStorage questionnaireStorage,
            IUserRepository users,
            IMemoryCache memoryCache)
        {
            this.auditLog = auditLog;
            this.questionnaireStorage = questionnaireStorage;
            this.users = users;
            this.memoryCache = memoryCache;
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
            progressReporting[processId] = new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo,
                0, 0, 
                new List<AssignmentUpgradeError>(), AssignmentUpgradeStatus.Queued);
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
                //extend life of cancellation token
                memoryCache.TryGetValue(GetCacheKey(processId), out CancellationTokenSource source);
                return progressReporting[processId];
            }

            return null;
        }

        public CancellationToken GetCancellationToken(Guid processId)
        {
            var source = memoryCache.Set(GetCacheKey(processId),  
                new CancellationTokenSource(),
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60)));

            return source.Token;
        }

        public void StopProcess(Guid processId)
        {
            if (memoryCache.TryGetValue(GetCacheKey(processId), out CancellationTokenSource source))
            {
                source.Cancel();
            }
        }
        
        private string GetCacheKey(Guid id)=> "AssignmentsUpgradeService-" + id;

    }
}
