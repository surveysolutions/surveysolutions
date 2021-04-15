using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
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
        private readonly IScheduledTask<UpgradeAssignmentJob, AssignmentsUpgradeProcess> scheduler;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<AssignmentsUpgradeService> logger;

        public AssignmentsUpgradeService(ISystemLog auditLog, 
            IQuestionnaireStorage questionnaireStorage,
            IUserRepository users,
            IScheduledTask<UpgradeAssignmentJob, AssignmentsUpgradeProcess> scheduler,
            ILogger<AssignmentsUpgradeService> logger,
            IMemoryCache memoryCache)
        {
            this.auditLog = auditLog;
            this.questionnaireStorage = questionnaireStorage;
            this.users = users;
            this.scheduler = scheduler;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        public async Task EnqueueUpgrade(Guid processId,
            Guid userId,
            QuestionnaireIdentity migrateFrom,
            QuestionnaireIdentity migrateTo, CancellationToken token = default)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null)
                ?? throw new ArgumentException($@"Cannot find questionnaire {migrateTo} to migrate to", nameof(migrateTo));

            var user = await this.users.FindByIdAsync(userId, token);
            this.auditLog.AssignmentsUpgradeStarted(questionnaire.Title, migrateFrom.Version, migrateTo.Version, userId, user.UserName);

            var upgrade = new AssignmentsUpgradeProcess(processId, userId, migrateFrom, migrateTo);
            await scheduler.Schedule(upgrade);

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
    }
}
