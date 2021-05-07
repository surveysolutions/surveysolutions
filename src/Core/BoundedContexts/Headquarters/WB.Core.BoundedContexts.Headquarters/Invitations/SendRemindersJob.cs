using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;


namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendRemindersJob : IJob
    {
        private readonly ILogger<SendRemindersJob> logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;
        private readonly IWebInterviewEmailRenderer webInterviewEmailRenderer;
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly IWebInterviewLinkProvider interviewLinkProvider;
       
        public SendRemindersJob(
            ILogger<SendRemindersJob> logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage, 
            IWebInterviewEmailRenderer webInterviewEmailRenderer,
            IInScopeExecutor inScopeExecutor,
            IWebInterviewLinkProvider interviewLinkProvider)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
            this.inScopeExecutor = inScopeExecutor;
            this.interviewLinkProvider = interviewLinkProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!emailService.IsConfigured())
                    return;

                var questionnaires = invitationService.GetQuestionnairesWithInvitations().ToList();
                
                var sw = new Stopwatch();
                sw.Start();

                ISenderInformation senderInfo = emailService.GetSenderInfo();

                foreach (QuestionnaireBrowseItem questionnaire in questionnaires)
                {
                    var questionnaireIdentity = questionnaire.Identity();
                    WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(questionnaireIdentity);
                    
                    if(webInterviewConfig == null || !webInterviewConfig.Started)
                        continue;

                    await SendNoResponseReminder(questionnaireIdentity, 
                            questionnaire.Title, webInterviewConfig, senderInfo)
                        .ConfigureAwait(false);

                    await SendPartialResponseReminder(questionnaireIdentity, 
                            questionnaire.Title, webInterviewConfig, senderInfo)
                        .ConfigureAwait(false);

                    await SendRejectedInterviewReminder(questionnaireIdentity, 
                            questionnaire.Title, webInterviewConfig, senderInfo)
                        .ConfigureAwait(false);
                }

                sw.Stop();
            }
            catch (OperationCanceledException)
            {
                this.logger.LogWarning("Reminders distribution job: CANCELED");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Reminders distribution job: FAILED");
            }
        }

        private async Task SendRejectedInterviewReminder(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle,
            WebInterviewConfig webInterviewConfig,
            ISenderInformation senderInfo)
        {
            var invitationIdsToSend = invitationService.GetInvitationsWithRejectedInterview(questionnaireIdentity);
            
            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.RejectEmail);

            foreach (var invitationId in invitationIdsToSend)
            {
                Invitation invitation = invitationService.GetInvitation(invitationId);
                var password = invitation.Assignment.Password;
                var address = invitation.Assignment.Email;

                var link = interviewLinkProvider.WebInterviewContinueLink(invitation);
                var emailContent = new EmailContent(emailTemplate, questionnaireTitle, link, password);

                var emailParamsId = $"{Guid.NewGuid():N}-{invitationId}-Reject";
                var emailParams = new EmailParameters
                {
                    Id = emailParamsId,
                    AssignmentId = invitation.AssignmentId,
                    InvitationId = invitation.Id,
                    Subject = emailContent.Subject,
                    LinkText = emailContent.LinkText,
                    MainText = emailContent.MainText,
                    PasswordDescription = emailContent.PasswordDescription,
                    Password = password,
                    Address = senderInfo.Address,
                    SurveyName = questionnaireTitle,
                    SenderName = senderInfo.SenderName,
                    Link = link
                };
                emailParamsStorage.Store(emailParams, emailParamsId);

                var interviewEmail = await webInterviewEmailRenderer.RenderEmail(emailParams).ConfigureAwait(false);

                await inScopeExecutor.ExecuteAsync(async (locator) =>
                {
                    var invitationServiceLocal = locator.GetInstance<IInvitationService>(); 
                    try
                    {
                        var emailId = await emailService.SendEmailAsync(address, 
                            emailParams.Subject,
                            interviewEmail.MessageHtml,
                            interviewEmail.MessageText).ConfigureAwait(false);

                        invitationServiceLocal.MarkRejectedInterviewReminderSent(invitationId, emailId);
                    }
                    catch (EmailServiceException e)
                    {
                        invitationServiceLocal.RejectedInterviewReminderWasNotSent(invitation.Id);
                        this.logger.LogError(e, "Rejected email was not sent");
                    }
                }).ConfigureAwait(false);
            }
        }

        private async Task SendPartialResponseReminder(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, WebInterviewConfig webInterviewConfig, ISenderInformation senderInfo)
        {
            if (!webInterviewConfig.ReminderAfterDaysIfPartialResponse.HasValue)
                return;

            var invitationIdsToRemind = invitationService.GetPartialResponseInvitations(questionnaireIdentity, webInterviewConfig.ReminderAfterDaysIfPartialResponse.Value);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_PartialResponse);

            await SendReminders(questionnaireTitle, invitationIdsToRemind, emailTemplate, senderInfo).ConfigureAwait(false);
        }

        private async Task SendNoResponseReminder(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, WebInterviewConfig webInterviewConfig, ISenderInformation senderInfo)
        {
            if (!webInterviewConfig.ReminderAfterDaysIfNoResponse.HasValue)
                return;

            var invitationIdsToRemind = invitationService.GetNoResponseInvitations(questionnaireIdentity, webInterviewConfig.ReminderAfterDaysIfNoResponse.Value);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_NoResponse);

            await SendReminders(questionnaireTitle, invitationIdsToRemind, emailTemplate, senderInfo).ConfigureAwait(false);
        }

        private async Task SendReminders(string questionnaireTitle, IEnumerable<int> invitationIdsToRemind,
            WebInterviewEmailTemplate emailTemplate, ISenderInformation senderInfo)
        {
            var surveyName = questionnaireTitle;
            foreach (var invitationId in invitationIdsToRemind)
            {
                Invitation invitation = invitationService.GetInvitation(invitationId);
                var password = invitation.Assignment.Password;
                var address = invitation.Assignment.Email;

                var link = this.interviewLinkProvider.WebInterviewStartLink(invitation);
                var emailContent = new EmailContent(emailTemplate, questionnaireTitle, link, password);

                var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitationId}";
                var emailParams = new EmailParameters
                {
                    Id = emailParamsId,
                    AssignmentId = invitation.AssignmentId,
                    InvitationId = invitation.Id,
                    Subject = emailContent.Subject,
                    LinkText = emailContent.LinkText,
                    MainText = emailContent.MainText,
                    PasswordDescription = emailContent.PasswordDescription,
                    Password = password,
                    Address = senderInfo.Address,
                    SurveyName = surveyName,
                    SenderName = senderInfo.SenderName,
                    Link = link
                };
                emailParamsStorage.Store(emailParams, emailParamsId);

                var interviewEmail = await webInterviewEmailRenderer.RenderEmail(emailParams).ConfigureAwait(false);

                try
                {
                    var emailId = await emailService.SendEmailAsync(address, emailParams.Subject, interviewEmail.MessageHtml, interviewEmail.MessageText)
                        .ConfigureAwait(false);

                    invitationService.MarkInvitationAsReminded(invitationId, emailId);
                }
                catch (EmailServiceException e)
                {
                    invitationService.ReminderWasNotSent(invitationId, invitation.AssignmentId, address, e.Message);
                    this.logger.LogError(e, "Interview email was not sent");
                }
            }
        }
    }

    public class SendRemindersTask : BaseTask
    {
        public SendRemindersTask(ISchedulerFactory schedulerFactory) : base(schedulerFactory, "Send reminders", typeof(SendRemindersJob)) { }
    }
}
