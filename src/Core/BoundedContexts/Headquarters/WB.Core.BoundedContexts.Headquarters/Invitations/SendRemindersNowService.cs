using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Workspaces;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface ISendRemindersNowService
    {
        Task<SendRemindersNowStatus> SendAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle,
            EmailTextTemplateType type, Guid requestId);
        Task<SendRemindersNowStatus> GetStatusAsync(QuestionnaireIdentity questionnaireIdentity, Guid requestId);
    }

    public class ReminderRequestIdConflictException : Exception
    {
        public ReminderRequestIdConflictException()
            : base("The request ID has already been used for a different reminder request.")
        {
        }
    }

    public class SendRemindersNowService : ISendRemindersNowService
    {
        private readonly IRootScopeExecutor rootScopeExecutor;
        private readonly IScheduledTask<SendRemindersNowJob, SendRemindersNowRequest> sendRemindersTask;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public SendRemindersNowService(
            IRootScopeExecutor rootScopeExecutor,
            IScheduledTask<SendRemindersNowJob, SendRemindersNowRequest> sendRemindersTask,
            ISchedulerFactory schedulerFactory,
            IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.rootScopeExecutor = rootScopeExecutor;
            this.sendRemindersTask = sendRemindersTask;
            this.schedulerFactory = schedulerFactory;
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        public async Task<SendRemindersNowStatus> SendAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, EmailTextTemplateType type, Guid requestId)
        {
            if (requestId == Guid.Empty)
                throw new ArgumentException("A request ID is required.", nameof(requestId));

            var status = await rootScopeExecutor.ExecuteAsync(async locator =>
            {
                var unitOfWork = locator.GetInstance<IUnitOfWork>();
                await ReminderEmailSendLock.AcquireRequestAsync(unitOfWork, questionnaireIdentity, requestId)
                    .ConfigureAwait(false);

                var storage = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>();
                var requestStatusKey = SendRemindersNowRequest.GetStatusStorageKey(requestId);
                var requestStatus = storage.GetByIdUncached(requestStatusKey);
                if (requestStatus != null)
                {
                    if (requestStatus.QuestionnaireIdentity != questionnaireIdentity || requestStatus.Type != type)
                        throw new ReminderRequestIdConflictException();

                    return requestStatus;
                }

                var aliases = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowRequestIdAlias>>();
                var aliasKey = SendRemindersNowRequestIdAlias.GetStorageKey(requestId);
                var alias = aliases.GetByIdUncached(aliasKey);
                if (alias != null)
                {
                    if (alias.QuestionnaireIdentity != questionnaireIdentity || alias.Type != type)
                        throw new ReminderRequestIdConflictException();

                    var aliasedStatus = storage.GetByIdUncached(
                        SendRemindersNowRequest.GetStatusStorageKey(alias.RequestId));
                    return aliasedStatus ?? throw new ReminderRequestIdConflictException();
                }

                var activeStatusKey = SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, type);
                var activeStatus = storage.GetByIdUncached(activeStatusKey);
                if (activeStatus?.State is SendRemindersNowState.Queued or SendRemindersNowState.InProgress)
                {
                    if (activeStatus.RequestId != requestId)
                    {
                        aliases.Store(new SendRemindersNowRequestIdAlias
                        {
                            RequestId = activeStatus.RequestId,
                            QuestionnaireIdentity = questionnaireIdentity,
                            Type = type
                        }, aliasKey);
                    }

                    return activeStatus;
                }

                var status = new SendRemindersNowStatus
                {
                    RequestId = requestId,
                    QuestionnaireIdentity = questionnaireIdentity,
                    QuestionnaireTitle = questionnaireTitle,
                    Type = type,
                    State = SendRemindersNowState.Queued
                };
                storage.Store(status, activeStatusKey);
                storage.Store(status, requestStatusKey);
                return status;
            }).ConfigureAwait(false);

            await EnsureScheduledAsync(status).ConfigureAwait(false);
            return status;
        }

        public async Task<SendRemindersNowStatus> GetStatusAsync(QuestionnaireIdentity questionnaireIdentity,
            Guid requestId)
        {
            var status = await rootScopeExecutor.ExecuteAsync(locator =>
            {
                var storage = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>();
                var status = storage.GetByIdUncached(SendRemindersNowRequest.GetStatusStorageKey(requestId));
                if (status == null)
                {
                    var alias = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowRequestIdAlias>>()
                        .GetByIdUncached(SendRemindersNowRequestIdAlias.GetStorageKey(requestId));
                    if (alias?.QuestionnaireIdentity == questionnaireIdentity)
                        status = storage.GetByIdUncached(
                            SendRemindersNowRequest.GetStatusStorageKey(alias.RequestId));
                }

                return Task.FromResult(status);
            }).ConfigureAwait(false);

            if (status?.QuestionnaireIdentity != questionnaireIdentity)
                return null;

            await EnsureScheduledAsync(status).ConfigureAwait(false);
            return status;
        }

        private async Task EnsureScheduledAsync(SendRemindersNowStatus status)
        {
            if (status == null)
                return;

            await rootScopeExecutor.ExecuteAsync(async locator =>
            {
                var unitOfWork = locator.GetInstance<IUnitOfWork>();
                await ReminderEmailSendLock.AcquireSchedulingAsync(unitOfWork, status.RequestId)
                    .ConfigureAwait(false);

                var storage = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>();
                var statusStorageKey = SendRemindersNowRequest.GetStatusStorageKey(status.RequestId);
                var currentStatus = storage.GetByIdUncached(statusStorageKey);
                if (currentStatus?.State is not (SendRemindersNowState.Queued or SendRemindersNowState.InProgress))
                    return true;

                var request = new SendRemindersNowRequest
                {
                    RequestId = currentStatus.RequestId,
                    QuestionnaireIdentity = currentStatus.QuestionnaireIdentity,
                    QuestionnaireTitle = currentStatus.QuestionnaireTitle,
                    Type = currentStatus.Type
                };

                if (await IsScheduledOrRunningAsync(request).ConfigureAwait(false))
                    return true;

                var startedAtUtc = currentStatus.StartedAtUtc;
                try
                {
                    await sendRemindersTask.Schedule(request).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    var latestStatus = storage.GetByIdUncached(statusStorageKey);
                    if (latestStatus?.State is not (SendRemindersNowState.Queued or SendRemindersNowState.InProgress)
                        || latestStatus.StartedAtUtc != startedAtUtc
                        || await IsScheduledOrRunningAsync(request).ConfigureAwait(false))
                        return true;

                    latestStatus.State = SendRemindersNowState.Failed;
                    latestStatus.Error = "The reminder emails could not be scheduled.";
                    status.State = latestStatus.State;
                    status.Error = latestStatus.Error;
                    storage.Store(latestStatus,
                        SendRemindersNowRequest.GetStorageKey(latestStatus.QuestionnaireIdentity, latestStatus.Type));
                    storage.Store(latestStatus, statusStorageKey);
                }

                return true;
            }).ConfigureAwait(false);
        }

        private async Task<bool> IsScheduledOrRunningAsync(SendRemindersNowRequest request)
        {
            var scheduler = await schedulerFactory.GetScheduler().ConfigureAwait(false);
            var jobKey = ScheduledTask<SendRemindersNowJob, SendRemindersNowRequest>.Key;
            var workspaceName = workspaceContextAccessor.CurrentWorkspace()?.Name;
            var executingJobs = await scheduler.QueryFireInstances(new FireInstanceQuery { Job = jobKey },
                CancellationToken.None).ConfigureAwait(false);
            foreach (var executingJob in executingJobs.Items)
            {
                if (!executingJob.JobKey.Equals(jobKey))
                    continue;

                var trigger = await scheduler.GetTrigger(executingJob.TriggerKey).ConfigureAwait(false);
                if (trigger != null && HasWorkspace(trigger.JobDataMap, workspaceName) &&
                    HasRequestId(trigger.JobDataMap.GetString(BaseTask.TaskDataKey), request.RequestId))
                    return true;
            }

            const int pageSize = 100;
            for (var skip = 0; ; skip += pageSize)
            {
                var triggerPage = await scheduler.QueryTriggers(new TriggerQuery
                {
                    Job = jobKey,
                    Skip = skip,
                    Take = pageSize
                }, CancellationToken.None).ConfigureAwait(false);
                if (triggerPage.Items.Count == 0)
                    return false;

                var triggers = await scheduler.GetTriggers(triggerPage.Items.Select(trigger => trigger.Key).ToArray(),
                    CancellationToken.None).ConfigureAwait(false);
                if (triggers.Any(trigger =>
                        HasWorkspace(trigger.JobDataMap, workspaceName) &&
                        HasRequestId(trigger.JobDataMap.GetString(BaseTask.TaskDataKey), request.RequestId)))
                    return true;

                if (!triggerPage.HasMore)
                    return false;
            }
        }

        private static bool HasWorkspace(JobDataMap jobData, string workspaceName) =>
            string.Equals(jobData.GetString(WorkspaceConstants.QuartzJobKey), workspaceName,
                StringComparison.Ordinal);

        private static bool HasRequestId(string serializedRequest, Guid requestId)
        {
            if (string.IsNullOrEmpty(serializedRequest))
                return false;

            try
            {
                return JsonSerializer.Deserialize<SendRemindersNowRequest>(serializedRequest)?.RequestId == requestId;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
