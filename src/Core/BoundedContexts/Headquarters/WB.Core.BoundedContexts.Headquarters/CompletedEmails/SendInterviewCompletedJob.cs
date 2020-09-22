#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendInterviewCompletedJob : IJob
    {
        private readonly ILogger<SendInterviewCompletedJob> logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly ICompletedEmailsQueue completedEmailsQueue;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IWebInterviewEmailRenderer webInterviewEmailRenderer;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPdfInterviewGenerator pdfInterviewGenerator;
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;

        public SendInterviewCompletedJob(
            ILogger<SendInterviewCompletedJob> logger, 
            IInvitationService invitationService, 
            IEmailService emailService, 
            ICompletedEmailsQueue completedEmailsQueue,
            IStatefulInterviewRepository interviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IWebInterviewEmailRenderer webInterviewEmailRenderer,
            IQuestionnaireStorage questionnaireStorage,
            IPdfInterviewGenerator pdfInterviewGenerator,
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage
            )
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.completedEmailsQueue = completedEmailsQueue;
            this.interviewRepository = interviewRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
            this.questionnaireStorage = questionnaireStorage;
            this.pdfInterviewGenerator = pdfInterviewGenerator;
            this.emailParamsStorage = emailParamsStorage;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!emailService.IsConfigured())
                    return;

                var interviewIds = completedEmailsQueue.GetInterviewIdsForSend();
                
                var sw = new Stopwatch();
                sw.Start();

                ISenderInformation senderInfo = emailService.GetSenderInfo();

                foreach (Guid interviewId in interviewIds)
                {
                    await SendInterviewCompleteEmail(interviewId, senderInfo)
                        .ConfigureAwait(false);
                }

                sw.Stop();
            }
            catch (OperationCanceledException)
            {
                this.logger.LogWarning("Send completed emails job: CANCELED");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Send completed emails job: FAILED");
            }
        }

        private async Task SendInterviewCompleteEmail(Guid interviewId,
            ISenderInformation senderInfo)
        {
            var interview = interviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
            {
                completedEmailsQueue.Remove(interviewId);
                return;
            }

            var questionnaireIdentity = interview.QuestionnaireIdentity;
            WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(questionnaireIdentity);
            if (!webInterviewConfig.EmailOnComplete)
            {
                completedEmailsQueue.Remove(interviewId);
                return;
            }

            var assignmentId = interview.GetAssignmentId();
            if (assignmentId == null)
            {
                completedEmailsQueue.Remove(interviewId);
                return;
            }

            var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);
            if (questionnaire == null)
            {
                completedEmailsQueue.Remove(interviewId);
                return;
            }
            
            List<EmailAttachment>? attachments = new List<EmailAttachment>();
            if (webInterviewConfig.AttachAnswersInEmail)
            {
                var stream = pdfInterviewGenerator.Generate(interview.Id, PdfView.Interviewer);
                if (stream == null)
                    throw new ArgumentException($"Failed to generate pdf for interview {interviewId}");

                await using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                var attachment = new EmailAttachment()
                {
                    Filename = interview.GetInterviewKey() + ".pdf",
                    ContentType = "application/pdf",
                    Base64String = Convert.ToBase64String(ms.ToArray()),
                };
                
                attachments = new List<EmailAttachment>() { attachment };
            }

            
            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.CompleteInterviewEmail);
            
            Invitation invitation = invitationService.GetInvitationByAssignmentId(assignmentId.Value);
            var address = invitation.Assignment.Email;
            var emailContent = new EmailContent(emailTemplate, questionnaire.Title, null, null);
            emailContent.RenderInterviewData(interview, questionnaire);
            attachments.AddRange(emailContent.Attachments);

            var emailParamsId = $"{Guid.NewGuid():N}-{invitation.Id}-Complete";
            var emailParams = new EmailParameters
            {
                Id = emailParamsId,
                AssignmentId = invitation.AssignmentId,
                InvitationId = invitation.Id,
                Subject = emailContent.Subject,
                LinkText = emailContent.LinkText,
                MainText = emailContent.MainText,
                PasswordDescription = emailContent.PasswordDescription,
                Password = null,
                Address = senderInfo.Address,
                SurveyName = questionnaire.Title,
                SenderName = senderInfo.SenderName,
                Link = null
            };
            emailParamsStorage.Store(emailParams, emailParamsId);

            var interviewEmail = await webInterviewEmailRenderer.RenderEmail(emailParams).ConfigureAwait(false);

            // await inScopeExecutor.ExecuteAsync(async (locator) =>
            // {
            try
            {
                var emailId = await emailService.SendEmailAsync(address, 
                    emailParams.Subject,
                    interviewEmail.MessageHtml,
                    interviewEmail.MessageText,
                    attachments).ConfigureAwait(false);

                completedEmailsQueue.Remove(interviewId);
            }
            catch (EmailServiceException e)
            {
                completedEmailsQueue.MarkAsFailedToSend(interviewId);
                this.logger.LogError(e, "Complete email was not sent {interviewId}", interviewId);
            }
            // }).ConfigureAwait(false);
        }
    }

    public class SendInterviewCompletedTask : BaseTask
    {
        public SendInterviewCompletedTask(IScheduler scheduler) 
            : base(scheduler, "Send interview completed emails", typeof(SendInterviewCompletedJob)) { }
    }
}
