using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AssignmentsUpgradeService> logger;

        public AssignmentsUpgradeService(ISystemLog auditLog, 
            IQuestionnaireStorage questionnaireStorage,
            IUserRepository users,
            ILogger<AssignmentsUpgradeService> logger,
            IMemoryCache memoryCache)
        {
            this.auditLog = auditLog;
            this.questionnaireStorage = questionnaireStorage;
            this.users = users;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        public void EnqueueUpgrade(Guid processId,
            Guid userId,
            QuestionnaireIdentity migrateFrom,
            QuestionnaireIdentity migrateTo)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null);

            var user = this.users.FindById(userId);
            this.auditLog.AssignmentsUpgradeStarted(questionnaire.Title, migrateFrom.Version, migrateTo.Version, userId, user.UserName);

            var upgradeQueue = memoryCache.GetOrCreate(GetQueueCacheKey(),q=>
            {
                ConcurrentQueue<QueuedUpgrade> upgradeQueue = new ConcurrentQueue<QueuedUpgrade>();
                q.Priority = CacheItemPriority.NeverRemove;
                return upgradeQueue;
            } );

            upgradeQueue.Enqueue(new QueuedUpgrade(processId, userId, migrateFrom, migrateTo));

            memoryCache.Set(GetStatusCacheKey(processId),  
                new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo,
                    0, 0, 
                    new List<AssignmentUpgradeError>(), AssignmentUpgradeStatus.Queued),
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60)));

            logger.LogInformation($"Upgrade assignments enqueued. From {migrateFrom} to {migrateTo}. Process: {processId}.");
        }

        public void ReportProgress(Guid processId, AssignmentUpgradeProgressDetails progressDetails)
        {
            memoryCache.Set(GetStatusCacheKey(processId),  
                progressDetails,
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60)));
        }

        public QueuedUpgrade DequeueUpgrade()
        {
            if (!memoryCache.TryGetValue(GetQueueCacheKey(),
                out ConcurrentQueue<QueuedUpgrade> upgradeQueue)) return null;

            if (upgradeQueue.IsEmpty) return null;

            return upgradeQueue.TryDequeue(out QueuedUpgrade request) ? request : null;
        }

        public AssignmentUpgradeProgressDetails Status(Guid processId)
        {
            if (!memoryCache.TryGetValue(GetStatusCacheKey(processId),
                out AssignmentUpgradeProgressDetails progress)) return null;
            
            //extend life of cancellation token
            memoryCache.TryGetValue(GetCancellationCacheKey(processId), out CancellationTokenSource source);
            return progress;
        }

        public CancellationToken GetCancellationToken(Guid processId)
        {
            var source = memoryCache.Set(GetCancellationCacheKey(processId),  
                new CancellationTokenSource(),
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60)));

            return source.Token;
        }

        public void StopProcess(Guid processId)
        {
            if (memoryCache.TryGetValue(GetCancellationCacheKey(processId), out CancellationTokenSource source))
            {
                source.Cancel();
            }
        }
        
        private string GetCancellationCacheKey(Guid id)=> "AssignmentsUpgradeService-c-" + id;
        private string GetStatusCacheKey(Guid id)=> "AssignmentsUpgradeService-s-" + id;
        private string GetQueueCacheKey()=> "AssignmentsUpgradeService-q";
    }
}
