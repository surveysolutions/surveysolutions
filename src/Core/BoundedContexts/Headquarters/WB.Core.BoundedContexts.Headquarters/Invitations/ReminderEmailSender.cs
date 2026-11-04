using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class ReminderEmailSender : IReminderEmailSender
    {
        private readonly ILogger<ReminderEmailSender> logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;
        private readonly IWebInterviewEmailRenderer webInterviewEmailRenderer;
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly IWebInterviewLinkProvider interviewLinkProvider;
        private readonly IUnitOfWork unitOfWork;

        public ReminderEmailSender(
            ILogger<ReminderEmailSender> logger,
            IInvitationService invitationService,
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage,
            IWebInterviewEmailRenderer webInterviewEmailRenderer,
            IInScopeExecutor inScopeExecutor,
            IWebInterviewLinkProvider interviewLinkProvider,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
            this.inScopeExecutor = inScopeExecutor;
            this.interviewLinkProvider = interviewLinkProvider;
            this.unitOfWork = unitOfWork;
        }

        public async Task<ReminderEmailSendResult> SendRemindersAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle,
            EmailTextTemplateType reminderType, int thresholdDays, DateTime? requestStartedAtUtc = null)
        {
            WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(questionnaireIdentity);
            if (webInterviewConfig == null || !webInterviewConfig.Started)
                return new ReminderEmailSendResult();

            if (!await ReminderEmailSendLock.TryAcquireDeliveryAsync(unitOfWork, questionnaireIdentity, reminderType)
                    .ConfigureAwait(false))
                throw new ReminderDeliveryInProgressException();

            ISenderInformation senderInfo = emailService.GetSenderInfo();
            var result = new ReminderEmailSendResult();

            switch (reminderType)
            {
                case EmailTextTemplateType.Reminder_NoResponse:
                    await SendNoResponseRemindersAsync(questionnaireIdentity, questionnaireTitle,
                        webInterviewConfig, senderInfo, thresholdDays, result, requestStartedAtUtc).ConfigureAwait(false);
                    break;
                case EmailTextTemplateType.Reminder_PartialResponse:
                    await SendPartialResponseRemindersAsync(questionnaireIdentity, questionnaireTitle,
                        webInterviewConfig, senderInfo, thresholdDays, result, requestStartedAtUtc).ConfigureAwait(false);
                    break;
                case EmailTextTemplateType.RejectEmail:
                    await SendRejectedInterviewRemindersAsync(questionnaireIdentity, questionnaireTitle,
                        webInterviewConfig, senderInfo, result).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentException($"Unsupported reminder type: {reminderType}", nameof(reminderType));
            }

            return result;
        }

        private async Task SendNoResponseRemindersAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, WebInterviewConfig webInterviewConfig, ISenderInformation senderInfo,
            int thresholdDays, ReminderEmailSendResult result, DateTime? requestStartedAtUtc)
        {
            if (!webInterviewConfig.ReminderAfterDaysIfNoResponse.HasValue && thresholdDays > 0)
                return;

            var invitationIdsToRemind = invitationService.GetNoResponseInvitations(questionnaireIdentity, thresholdDays);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_NoResponse);

            await SendRemindersAsync(questionnaireTitle, invitationIdsToRemind, emailTemplate, senderInfo, result,
                    requestStartedAtUtc)
                .ConfigureAwait(false);
        }

        private async Task SendPartialResponseRemindersAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, WebInterviewConfig webInterviewConfig, ISenderInformation senderInfo,
            int thresholdDays, ReminderEmailSendResult result, DateTime? requestStartedAtUtc)
        {
            if (!webInterviewConfig.ReminderAfterDaysIfPartialResponse.HasValue && thresholdDays > 0)
                return;

            var invitationIdsToRemind = invitationService.GetPartialResponseInvitations(questionnaireIdentity, thresholdDays);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.Reminder_PartialResponse);

            await SendRemindersAsync(questionnaireTitle, invitationIdsToRemind, emailTemplate, senderInfo, result,
                    requestStartedAtUtc)
                .ConfigureAwait(false);
        }

        private async Task SendRejectedInterviewRemindersAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, WebInterviewConfig webInterviewConfig, ISenderInformation senderInfo,
            ReminderEmailSendResult result)
        {
            var invitationIdsToSend = invitationService.GetInvitationsWithRejectedInterview(questionnaireIdentity);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.RejectEmail);

            foreach (var invitationId in invitationIdsToSend)
            {
                Invitation invitation = invitationService.GetInvitation(invitationId);
                var password = invitation.Assignment.Password;
                var address = invitation.Assignment.Email;
                if (string.IsNullOrWhiteSpace(address))
                    continue;

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
                    string emailId;
                    try
                    {
                        emailId = await emailService.SendEmailAsync(address,
                            emailParams.Subject,
                            interviewEmail.MessageHtml,
                            interviewEmail.MessageText).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        result.FailedCount++;
                        invitationServiceLocal.RejectedInterviewReminderWasNotSent(invitation.Id);
                        this.logger.LogError(e, "Rejected email was not sent");
                        return;
                    }

                    invitationServiceLocal.MarkRejectedInterviewReminderSent(invitationId, emailId);
                    result.SentCount++;
                }).ConfigureAwait(false);
            }
        }

        private async Task SendRemindersAsync(string questionnaireTitle, IEnumerable<int> invitationIdsToRemind,
            WebInterviewEmailTemplate emailTemplate, ISenderInformation senderInfo, ReminderEmailSendResult result,
            DateTime? requestStartedAtUtc = null)
        {
            var surveyName = questionnaireTitle;
            foreach (var invitationId in invitationIdsToRemind)
            {
                Invitation invitation = invitationService.GetInvitation(invitationId);
                if (requestStartedAtUtc.HasValue &&
                    invitation.LastReminderSentOnUtc >= requestStartedAtUtc.Value)
                    continue;

                var password = invitation.Assignment.Password;
                var address = invitation.Assignment.Email;
                if (string.IsNullOrEmpty(address))
                    continue;

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

                string emailId;
                try
                {
                    emailId = await emailService.SendEmailAsync(address, emailParams.Subject,
                            interviewEmail.MessageHtml, interviewEmail.MessageText)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    result.FailedCount++;
                    invitationService.ReminderWasNotSent(invitationId, invitation.AssignmentId, address, e.Message);
                    this.logger.LogError(e, $"Interview email was not sent for invitation {invitationId}.");
                    continue;
                }

                invitationService.MarkInvitationAsReminded(invitationId, emailId);
                result.SentCount++;
            }
        }
    }
}
