using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class SendRemindersNowRequest
    {
        public Guid RequestId { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string QuestionnaireTitle { get; set; }
        public EmailTextTemplateType Type { get; set; }

        public string StorageKey => GetStorageKey(QuestionnaireIdentity, Type);

        public string StatusStorageKey => GetStatusStorageKey(RequestId);

        public static string GetStorageKey(QuestionnaireIdentity questionnaireIdentity, EmailTextTemplateType type) =>
            $"SendRemindersNow-{questionnaireIdentity}-{type}";

        public static string GetStatusStorageKey(Guid requestId) => $"SendRemindersNow-{requestId}";
    }

    public class SendRemindersNowStatus : AppSetting
    {
        public Guid RequestId { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public EmailTextTemplateType Type { get; set; }
        public SendRemindersNowState State { get; set; }
        public string Error { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public string QuestionnaireTitle { get; set; }
        public DateTime? StartedAtUtc { get; set; }
    }

    public enum SendRemindersNowState
    {
        Queued,
        InProgress,
        Completed,
        Failed,
        PartiallyFailed
    }

    public class ReminderEmailDeliveryOutcomes : AppSetting
    {
        public int SentCount { get; set; }
        public int FailedCount { get; set; }

        public static string GetStorageKey(Guid requestId) => $"ReminderDeliveryOutcomes-{requestId:N}";

        public ReminderEmailSendResult GetResult() => new ReminderEmailSendResult
        {
            SentCount = SentCount,
            FailedCount = FailedCount
        };
    }

    public class ReminderEmailDeliveryCheckpoint : AppSetting
    {
        public bool Sent { get; set; }

        public static string GetStorageKey(Guid requestId, int invitationId) =>
            $"ReminderDeliveryCheckpoint-{requestId:N}-{invitationId}";
    }

    public class SendRemindersNowJob : IJob<SendRemindersNowRequest>
    {
        private readonly IRootScopeExecutor rootScopeExecutor;
        private readonly ILogger<SendRemindersNowJob> logger;

        public SendRemindersNowJob(
            IRootScopeExecutor rootScopeExecutor,
            ILogger<SendRemindersNowJob> logger)
        {
            this.rootScopeExecutor = rootScopeExecutor;
            this.logger = logger;
        }

        public async Task Execute(SendRemindersNowRequest request, IJobExecutionContext context)
        {
            await rootScopeExecutor.ExecuteAsync(async locator =>
            {
                // Keep the request lock until the final status commits, not just until delivery completes.
                if (!await ReminderEmailSendLock.TryAcquireExecutionAsync(locator.GetInstance<IUnitOfWork>(),
                        request.RequestId).ConfigureAwait(false))
                    return false;

                await ExecuteRequestAsync(request).ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
        }

        private async Task ExecuteRequestAsync(SendRemindersNowRequest request)
        {
            var previousStatus = await GetStatusAsync(request.RequestId).ConfigureAwait(false);
            if (previousStatus?.State is SendRemindersNowState.Completed
                or SendRemindersNowState.Failed
                or SendRemindersNowState.PartiallyFailed)
                return;

            var status = previousStatus ?? new SendRemindersNowStatus
            {
                RequestId = request.RequestId,
                QuestionnaireIdentity = request.QuestionnaireIdentity,
                Type = request.Type,
                QuestionnaireTitle = request.QuestionnaireTitle
            };
            status.State = SendRemindersNowState.InProgress;
            status.StartedAtUtc ??= DateTime.UtcNow;
            await StoreStatusAsync(status, request.StorageKey).ConfigureAwait(false);

            try
            {
                var result = await rootScopeExecutor.ExecuteAsync(locator =>
                    locator.GetInstance<IReminderEmailSender>().SendRemindersAsync(request.QuestionnaireIdentity,
                        request.QuestionnaireTitle, request.Type, 0, status.StartedAtUtc, request.RequestId)).ConfigureAwait(false);
                status.SentCount = result.SentCount;
                status.FailedCount = result.FailedCount;
                status.State = result.FailedCount == 0
                    ? SendRemindersNowState.Completed
                    : result.SentCount == 0
                        ? SendRemindersNowState.Failed
                        : SendRemindersNowState.PartiallyFailed;
                if (result.FailedCount > 0)
                    status.Error = result.SentCount == 0
                        ? "The reminder emails could not be sent."
                        : "Some reminder emails could not be sent.";
            }
            catch (Exception exception)
            {
                var result = await rootScopeExecutor.ExecuteAsync(locator => Task.FromResult(
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>()
                        .GetByIdUncached(ReminderEmailDeliveryOutcomes.GetStorageKey(request.RequestId))?.GetResult()
                    ?? new ReminderEmailSendResult())).ConfigureAwait(false);
                status.SentCount = result.SentCount;
                status.FailedCount = result.FailedCount;
                status.State = result.SentCount == 0
                    ? SendRemindersNowState.Failed
                    : SendRemindersNowState.PartiallyFailed;
                status.Error = exception is ReminderDeliveryInProgressException
                    ? exception.Message
                    : result.SentCount == 0
                        ? "The reminder emails could not be sent."
                        : "Some reminder emails could not be sent.";
                logger.LogError(exception, "Immediate reminder email sending failed for request {RequestId}.",
                    request.RequestId);
            }

            await StoreStatusAsync(status, request.StorageKey).ConfigureAwait(false);
        }

        private async Task<SendRemindersNowStatus> GetStatusAsync(Guid requestId) =>
            await rootScopeExecutor.ExecuteAsync(locator =>
            {
                var storage = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>();
                return Task.FromResult(storage.GetByIdUncached(SendRemindersNowRequest.GetStatusStorageKey(requestId)));
            }).ConfigureAwait(false);

        private async Task StoreStatusAsync(SendRemindersNowStatus status, string storageKey)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    await rootScopeExecutor.ExecuteAsync(locator =>
                    {
                        var storage = locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>();
                        storage.Store(status, SendRemindersNowRequest.GetStatusStorageKey(status.RequestId));
                        storage.Store(status, storageKey);
                        return Task.FromResult(true);
                    }).ConfigureAwait(false);
                    return;
                }
                catch (Exception exception) when (attempt == 0)
                {
                    logger.LogWarning(exception, "Retrying reminder request status persistence for request {RequestId}.",
                        status.RequestId);
                }
            }
        }
    }
}
