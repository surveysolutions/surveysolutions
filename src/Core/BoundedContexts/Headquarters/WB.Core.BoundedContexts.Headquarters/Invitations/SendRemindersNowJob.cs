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

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class SendRemindersNowRequest
    {
        public Guid RequestId { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string QuestionnaireTitle { get; set; }
        public EmailTextTemplateType Type { get; set; }

        public string StorageKey => GetStorageKey(QuestionnaireIdentity, Type);

        public static string GetStorageKey(QuestionnaireIdentity questionnaireIdentity, EmailTextTemplateType type) =>
            $"SendRemindersNow-{questionnaireIdentity}-{type}";
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
    }

    public enum SendRemindersNowState
    {
        Queued,
        InProgress,
        Completed,
        Failed,
        PartiallyFailed
    }

    [DisallowConcurrentExecution]
    public class SendRemindersNowJob : IJob<SendRemindersNowRequest>
    {
        private readonly IReminderEmailSender reminderEmailSender;
        private readonly IRootScopeExecutor rootScopeExecutor;
        private readonly ILogger<SendRemindersNowJob> logger;

        public SendRemindersNowJob(
            IReminderEmailSender reminderEmailSender,
            IRootScopeExecutor rootScopeExecutor,
            ILogger<SendRemindersNowJob> logger)
        {
            this.reminderEmailSender = reminderEmailSender;
            this.rootScopeExecutor = rootScopeExecutor;
            this.logger = logger;
        }

        public async Task Execute(SendRemindersNowRequest request, IJobExecutionContext context)
        {
            var status = new SendRemindersNowStatus
            {
                RequestId = request.RequestId,
                QuestionnaireIdentity = request.QuestionnaireIdentity,
                Type = request.Type
            };
            status.State = SendRemindersNowState.InProgress;
            await StoreStatusAsync(status, request.StorageKey).ConfigureAwait(false);

            try
            {
                var result = await reminderEmailSender.SendRemindersAsync(request.QuestionnaireIdentity,
                    request.QuestionnaireTitle, request.Type, 0).ConfigureAwait(false);
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
                status.State = SendRemindersNowState.Failed;
                status.Error = "The reminder emails could not be sent.";
                logger.LogError(exception, "Immediate reminder email sending failed for request {RequestId}.",
                    request.RequestId);
            }

            await StoreStatusAsync(status, request.StorageKey).ConfigureAwait(false);
        }

        private Task StoreStatusAsync(SendRemindersNowStatus status, string storageKey) =>
            rootScopeExecutor.ExecuteAsync(locator =>
            {
                locator.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>().Store(status, storageKey);
                return Task.FromResult(true);
            });
    }
}
