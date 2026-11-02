using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class SendRemindersNowRequest
    {
        public Guid RequestId { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string QuestionnaireTitle { get; set; }
        public EmailTextTemplateType Type { get; set; }

        public string StorageKey => RequestId.ToString("N");
    }

    public class SendRemindersNowStatus
    {
        public Guid RequestId { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public EmailTextTemplateType Type { get; set; }
        public SendRemindersNowState State { get; set; }
        public string Error { get; set; }
    }

    public enum SendRemindersNowState
    {
        Queued,
        InProgress,
        Completed,
        Failed
    }

    [DisallowConcurrentExecution]
    public class SendRemindersNowJob : IJob<SendRemindersNowRequest>
    {
        private readonly IReminderEmailSender reminderEmailSender;
        private readonly IPlainKeyValueStorage<SendRemindersNowStatus> statusStorage;
        private readonly ILogger<SendRemindersNowJob> logger;

        public SendRemindersNowJob(
            IReminderEmailSender reminderEmailSender,
            IPlainKeyValueStorage<SendRemindersNowStatus> statusStorage,
            ILogger<SendRemindersNowJob> logger)
        {
            this.reminderEmailSender = reminderEmailSender;
            this.statusStorage = statusStorage;
            this.logger = logger;
        }

        public async Task Execute(SendRemindersNowRequest request, IJobExecutionContext context)
        {
            var status = statusStorage.GetById(request.StorageKey) ?? new SendRemindersNowStatus
            {
                RequestId = request.RequestId,
                QuestionnaireIdentity = request.QuestionnaireIdentity,
                Type = request.Type
            };
            status.State = SendRemindersNowState.InProgress;
            statusStorage.Store(status, request.StorageKey);

            try
            {
                await reminderEmailSender.SendRemindersAsync(request.QuestionnaireIdentity,
                    request.QuestionnaireTitle, request.Type, 0).ConfigureAwait(false);
                status.State = SendRemindersNowState.Completed;
            }
            catch (Exception exception)
            {
                status.State = SendRemindersNowState.Failed;
                status.Error = "The reminder emails could not be sent.";
                logger.LogError(exception, "Immediate reminder email sending failed for request {RequestId}.",
                    request.RequestId);
            }

            statusStorage.Store(status, request.StorageKey);
        }
    }
}
