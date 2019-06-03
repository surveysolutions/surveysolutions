﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpStatistician httpStatistician;

        public SendRemindersJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage, 
            IHttpClientFactory httpClientFactory, 
            IHttpStatistician httpStatistician)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
            this.httpClientFactory = httpClientFactory;
            this.httpStatistician = httpStatistician;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!emailService.IsConfigured())
                    return Task.CompletedTask;

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

            return Task.CompletedTask;
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
                var emailContent = new EmailContent(emailTemplate, questionnaireTitle, link, password);

                var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitationId}";
                var emailParams = new EmailParameters
                {
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

                var client = httpClientFactory.CreateClient(httpStatistician);
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
