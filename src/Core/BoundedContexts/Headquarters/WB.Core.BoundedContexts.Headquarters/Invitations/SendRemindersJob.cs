using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendRemindersJob : IJob
    {
        private readonly ILogger logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;

        public SendRemindersJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                if (!emailService.IsConfigured())
                    return;

                var questionnaires = invitationService.GetQuestionnairesWithInvitations().ToList();
                

                this.logger.Debug("Reminders distribution job: Started");

                var sw = new Stopwatch();
                sw.Start();

                ISenderInformation senderInfo = emailService.GetSenderInfo();

                foreach (QuestionnaireLiteViewItem questionnaire in questionnaires)
                {
                    var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaire.Id);
                    WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(questionnaireIdentity);
                    var baseUrl = webInterviewConfig.BaseUrl;
                    
                    SendNoResponseReminder(questionnaireIdentity, questionnaire.Title, webInterviewConfig, baseUrl, senderInfo);

                    SendPartialResponseReminder(questionnaireIdentity, questionnaire.Title, webInterviewConfig, baseUrl, senderInfo);
                }

                sw.Stop();
                this.logger.Debug($"Reminders distribution job: Finished. Elapsed time: {sw.Elapsed}");
            }
            catch (OperationCanceledException)
            {
                this.logger.Error($"Reminders distribution job: CANCELED.");
            }
            catch (Exception ex)
            {
                this.logger.Error($"Reminders distribution job: FAILED. Reason: {ex.Message} ", ex);
            }
        }

        private void SendPartialResponseReminder(QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle, WebInterviewConfig webInterviewConfig, string baseUrl, ISenderInformation senderInfo)
        {
            if (!webInterviewConfig.ReminderAfterDaysIfPartialResponse.HasValue)
                return;

            var invitationIdsToRemind = invitationService.GetPartialResponseInvitations(questionnaireIdentity, webInterviewConfig.ReminderAfterDaysIfPartialResponse.Value);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_PartialResponse);

            SendReminders(questionnaireTitle, baseUrl, invitationIdsToRemind, emailTemplate, senderInfo);
        }

        private void SendNoResponseReminder(QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle, WebInterviewConfig webInterviewConfig, string baseUrl, ISenderInformation senderInfo)
        {
            if (!webInterviewConfig.ReminderAfterDaysIfNoResponse.HasValue)
                return;

            var invitationIdsToRemind = invitationService.GetNoResponseInvitations(questionnaireIdentity, webInterviewConfig.ReminderAfterDaysIfNoResponse.Value);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_NoResponse);

            SendReminders(questionnaireTitle, baseUrl, invitationIdsToRemind, emailTemplate, senderInfo);
        }

        private void SendReminders(string questionnaireTitle, string baseUrl, IEnumerable<int> invitationIdsToRemind,
            WebInterviewEmailTemplate emailTemplate, ISenderInformation senderInfo)
        {
            var surveyName = questionnaireTitle;
            foreach (var invitationId in invitationIdsToRemind)
            {
                Invitation invitation = invitationService.GetInvitation(invitationId);
                var password = invitation.Assignment.Password;
                var address = invitation.Assignment.Email;

                var link = $"{baseUrl}/WebInterview/{invitation.Token}/Start";


                var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitationId}";
                var emailParams = new EmailParameters
                {
                    AssignmentId = invitation.AssignmentId,
                    InvitationId = invitation.Id,
                    Subject = emailTemplate.Subject
                        .Replace(WebInterviewEmailTemplate.SurveyName, surveyName),
                    LinkText = emailTemplate.LinkText,
                    MainText = emailTemplate.MainText
                        .Replace(WebInterviewEmailTemplate.SurveyName, surveyName),
                    PasswordDescription = emailTemplate.PasswordDescription,
                    Password = password,
                    Address = senderInfo.Address,
                    SurveyName = surveyName,
                    SenderName = senderInfo.SenderName,
                    Link = link
                };
                emailParamsStorage.Store(emailParams, emailParamsId);

                var client = new HttpClient{};
                var htmlMessage = client.GetStringAsync($"{baseUrl}/WebEmails/Html/{emailParamsId}").Result ?? string.Empty;
                var textMessage = client.GetStringAsync($"{baseUrl}/WebEmails/Text/{emailParamsId}/").Result ?? string.Empty;

                try
                {
                    var sendEmailTask = emailService.SendEmailAsync(address, emailParams.Subject, htmlMessage, textMessage);

                    var emailId = sendEmailTask.Result;

                    invitationService.MarkInvitationAsReminded(invitationId, emailId);
                }
                catch (EmailServiceException e)
                {
                    invitationService.ReminderWasNotSent(invitationId, invitation.AssignmentId, address, e.Message);
                }
            }
        }
    }

    public class SendRemindersTask : BaseTask
    {
        public SendRemindersTask(IScheduler scheduler) : base(scheduler, "Send reminders", typeof(SendRemindersJob)) { }
    }
}
