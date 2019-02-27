using System;
using System.Diagnostics;
using System.Linq;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendRemindersJob : IJob
    {
        private readonly ILogger logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesStorage;

        public SendRemindersJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnairesStorage = questionnaires;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var questionnaires = questionnairesStorage.Query(_ => _.Where(x => !x.IsDeleted).ToList());
                var baseUrl = "";

                this.logger.Debug("Reminders distribution job: Started");

                var sw = new Stopwatch();
                sw.Start();

                foreach (QuestionnaireBrowseItem questionnaire in questionnaires)
                {
                    WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(questionnaire.Identity());
                    if (webInterviewConfig.ReminderAfterDaysIfNoResponse.HasValue)
                    {
                        SendNoResponseReminder(questionnaire, webInterviewConfig, baseUrl);
                    }

                    if (webInterviewConfig.ReminderAfterDaysIfPartialResponse.HasValue)
                    {
                        SendPartialResponseReminder(questionnaire, webInterviewConfig, baseUrl);
                    }
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

        private void SendPartialResponseReminder(QuestionnaireBrowseItem questionnaire, WebInterviewConfig webInterviewConfig, string baseUrl)
        {
            var invitationIdsToRemind = invitationService.GetPartialResponseInvitations(questionnaire.Identity(), webInterviewConfig.ReminderAfterDaysIfPartialResponse.Value);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_PartialResponse);

            var surveyName = questionnaire.Title;

            foreach (var invitationId in invitationIdsToRemind)
            {
                Invitation invitation = invitationService.GetInvitation(invitationId);
                var password = invitation.Assignment.Password;
                var address = invitation.Assignment.Email;

                var link = $"{baseUrl}/{invitation.Token}/Start";

                var hasPassword = !string.IsNullOrWhiteSpace(password);

                var email = PersonalizedWebInterviewEmail.FromTemplate(emailTemplate)
                    .SubstitutePassword(password)
                    .SubstituteLink(link)
                    .SubstituteSurveyName(surveyName);

                try
                {
                    var emailId = emailService.SendEmailAsync(address, email.Subject,
                        hasPassword ? email.MessageWithPasswordHtml : email.MessageHtml,
                        hasPassword ? email.MessageWithPassword : email.Message).Result;
                    invitationService.MarkInvitationAsReminded(invitationId, emailId);
                }
                catch (EmailServiceException)
                {
                    //invitationService.InvitationWasNotSent(invitationId, invitation.AssignmentId, address, e.Message);
                }
            }
        }

        private void SendNoResponseReminder(QuestionnaireBrowseItem questionnaire, WebInterviewConfig webInterviewConfig, string baseUrl)
        {
            var invitationIdsToRemind = invitationService.GetNoResponseInvitations(questionnaire.Identity(), webInterviewConfig.ReminderAfterDaysIfNoResponse.Value);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_NoResponse);

            var surveyName = questionnaire.Title;

            foreach (var invitationId in invitationIdsToRemind)
            {
                Invitation invitation = invitationService.GetInvitation(invitationId);
                var password = invitation.Assignment.Password;
                var address = invitation.Assignment.Email;
                var link = $"{baseUrl}/{invitation.Token}/Start";

                var hasPassword = !string.IsNullOrWhiteSpace(password);

                var email = PersonalizedWebInterviewEmail.FromTemplate(emailTemplate)
                    .SubstitutePassword(password)
                    .SubstituteLink(link)
                    .SubstituteSurveyName(surveyName);

                try
                {
                    var emailId = emailService.SendEmailAsync(address, email.Subject,
                        hasPassword ? email.MessageWithPasswordHtml : email.MessageHtml,
                        hasPassword ? email.MessageWithPassword : email.Message).Result;
                    invitationService.MarkInvitationAsReminded(invitationId, emailId);
                }
                catch (EmailServiceException e)
                {
                    //invitationService.InvitationWasNotSent(invitationId, invitation.AssignmentId, address, e.Message);
                }
            }
        }
    }

    public class SendRemindersTask : BaseTask
    {
        public SendRemindersTask(IScheduler scheduler) : base(scheduler, "Send reminders", typeof(SendRemindersJob)) { }
    }
}
