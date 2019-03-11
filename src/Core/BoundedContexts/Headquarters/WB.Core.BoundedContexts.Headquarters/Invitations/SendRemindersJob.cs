using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly IWebInterviewEmailRenderer emailRenderer;

        public SendRemindersJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IWebInterviewEmailRenderer emailRenderer)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailRenderer = emailRenderer;
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

                var link = $"{baseUrl}/{invitation.Token}/Start";

                var email = emailRenderer.RenderEmail(emailTemplate, password, link, surveyName, senderInfo.Address, senderInfo.SenderName);

                try
                {
                    var sendEmailTask = emailService.SendEmailAsync(address, email.Subject, email.MessageHtml, email.MessageText);

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
