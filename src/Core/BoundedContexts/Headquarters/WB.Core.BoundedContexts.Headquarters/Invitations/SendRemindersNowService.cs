using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface ISendRemindersNowService
    {
        Task<SendRemindersNowStatus> SendAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle,
            EmailTextTemplateType type);
        Task<SendRemindersNowStatus> GetStatusAsync(QuestionnaireIdentity questionnaireIdentity, Guid requestId);
    }

    public class SendRemindersNowService : ISendRemindersNowService
    {
        private readonly IRootScopeExecutor rootScopeExecutor;
        private readonly IScheduledTask<SendRemindersNowJob, SendRemindersNowRequest> sendRemindersTask;
        private readonly ISchedulerFactory schedulerFactory;

        public SendRemindersNowService(
            IRootScopeExecutor rootScopeExecutor,
            IScheduledTask<SendRemindersNowJob, SendRemindersNowRequest> sendRemindersTask,
            ISchedulerFactory schedulerFactory)
        {
            this.rootScopeExecutor = rootScopeExecutor;
            this.sendRemindersTask = sendRemindersTask;
            this.schedulerFactory = schedulerFactory;
        }

        public async Task<SendRemindersNowStatus> SendAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, EmailTextTemplateType type)
        {
            var status = await rootScopeExecutor.ExecuteAsync(async locator =>
            {
                var unitOfWork = locator.GetInstance<IUnitOfWork>();
                await ReminderEmailSendLock.AcquireRequestAsync(unitOfWork, questionnaireIdentity, type)
                    .ConfigureAwait(false);

                var storage = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>();
                var activeStatusKey = SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, type);
                var activeStatus = storage.GetById(activeStatusKey);
                if (activeStatus?.State is SendRemindersNowState.Queued or SendRemindersNowState.InProgress)
                    return activeStatus;

                var status = new SendRemindersNowStatus
                {
                    RequestId = Guid.NewGuid(),
                    QuestionnaireIdentity = questionnaireIdentity,
                    QuestionnaireTitle = questionnaireTitle,
                    Type = type,
                    State = SendRemindersNowState.Queued
                };
                storage.Store(status, activeStatusKey);
                storage.Store(status, SendRemindersNowRequest.GetStatusStorageKey(status.RequestId));
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
                return Task.FromResult(storage.GetById(SendRemindersNowRequest.GetStatusStorageKey(requestId)));
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
                var currentStatus = storage.GetById(statusStorageKey);
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
                    var latestStatus = storage.GetById(statusStorageKey);
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
            var executingJobs = await scheduler.GetCurrentlyExecutingJobs().ConfigureAwait(false);
            if (executingJobs.Any(job => HasRequestId(job.MergedJobDataMap.GetString(BaseTask.TaskDataKey),
                    request.RequestId)))
                return true;

            var triggers = await scheduler.GetTriggersOfJob(
                ScheduledTask<SendRemindersNowJob, SendRemindersNowRequest>.Key).ConfigureAwait(false);
            return triggers.Any(trigger =>
                HasRequestId(trigger.JobDataMap.GetString(BaseTask.TaskDataKey), request.RequestId));
        }

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
