﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public class QueuedUpgrade
    {
        public QueuedUpgrade(Guid processId, Guid userId, QuestionnaireIdentity @from, QuestionnaireIdentity to)
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

    internal class AssignmentsUpgradeService : IAssignmentsUpgradeService
    {
        private readonly ISystemLog auditLog;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private static readonly Dictionary<Guid, AssignmentUpgradeProgressDetails> progressReporting = new Dictionary<Guid, AssignmentUpgradeProgressDetails>();
        private static readonly ConcurrentQueue<QueuedUpgrade> upgradeQueue = new ConcurrentQueue<QueuedUpgrade>();
        private readonly Dictionary<Guid, CancellationTokenSource> cancellationTokens = new Dictionary<Guid, CancellationTokenSource>();

        public AssignmentsUpgradeService(ISystemLog auditLog, IQuestionnaireStorage questionnaireStorage)
        {
            this.auditLog = auditLog;
            this.questionnaireStorage = questionnaireStorage;
        }

        public void EnqueueUpgrade(Guid processId, Guid userId, QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null);

            this.auditLog.AssignmentsUpgradeStarted(questionnaire.Title, migrateFrom.Version, migrateTo.Version);

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
