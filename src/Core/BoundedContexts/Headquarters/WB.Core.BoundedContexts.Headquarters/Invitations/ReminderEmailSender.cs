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
        private readonly IPlainKeyValueStorage<WebInterviewConfig> webInterviewConfigs;
        private readonly IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes> deliveryOutcomes;
        private readonly IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint> deliveryCheckpoints;
        private readonly IWebInterviewEmailRenderer webInterviewEmailRenderer;
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly IWebInterviewLinkProvider interviewLinkProvider;
        private readonly IUnitOfWork unitOfWork;

        public ReminderEmailSender(
            ILogger<ReminderEmailSender> logger,
            IInvitationService invitationService,
            IEmailService emailService,
            IPlainKeyValueStorage<WebInterviewConfig> webInterviewConfigs,
            IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes> deliveryOutcomes,
            IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint> deliveryCheckpoints,
            IWebInterviewEmailRenderer webInterviewEmailRenderer,
            IInScopeExecutor inScopeExecutor,
            IWebInterviewLinkProvider interviewLinkProvider,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigs = webInterviewConfigs;
            this.deliveryOutcomes = deliveryOutcomes;
            this.deliveryCheckpoints = deliveryCheckpoints;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
            this.inScopeExecutor = inScopeExecutor;
            this.interviewLinkProvider = interviewLinkProvider;
            this.unitOfWork = unitOfWork;
        }

        public async Task<ReminderEmailSendResult> SendRemindersAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, EmailTextTemplateType reminderType, int thresholdDays,
            DateTime? requestStartedAtUtc = null, Guid? requestId = null)
        {
            if (!await ReminderEmailSendLock.TryAcquireDeliveryAsync(unitOfWork, questionnaireIdentity, reminderType)
                    .ConfigureAwait(false))
                throw new ReminderDeliveryInProgressException();

            var outcomes = requestId.HasValue
                ? deliveryOutcomes.GetByIdUncached(ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value))
                    ?? new ReminderEmailDeliveryOutcomes()
                : new ReminderEmailDeliveryOutcomes();
            var result = outcomes.GetResult();
            var config = webInterviewConfigs.GetByIdUncached(questionnaireIdentity.ToString());
            if (config == null || !config.Started)
                return result;

            IEnumerable<int> invitationIds;
            switch (reminderType)
            {
                case EmailTextTemplateType.Reminder_NoResponse:
                    if (!config.ReminderAfterDaysIfNoResponse.HasValue && thresholdDays > 0)
                        return result;
                    invitationIds = invitationService.GetNoResponseInvitations(questionnaireIdentity, thresholdDays);
                    break;
                case EmailTextTemplateType.Reminder_PartialResponse:
                    if (!config.ReminderAfterDaysIfPartialResponse.HasValue && thresholdDays > 0)
                        return result;
                    invitationIds = invitationService.GetPartialResponseInvitations(questionnaireIdentity, thresholdDays);
                    break;
                case EmailTextTemplateType.RejectEmail:
                    invitationIds = invitationService.GetInvitationsWithRejectedInterview(questionnaireIdentity);
                    break;
                default:
                    throw new ArgumentException($"Unsupported reminder type: {reminderType}", nameof(reminderType));
            }

            var senderInfo = emailService.GetSenderInfo();
            var emailTemplate = config.GetEmailTemplate(reminderType);
            foreach (var invitationId in invitationIds)
            {
                // Request checkpoints, unlike reminder timestamps, also retain failed deliveries and recipients
                // that no longer match the live eligibility query after a restart.
                if (requestId.HasValue && deliveryCheckpoints.GetByIdUncached(
                        ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId.Value, invitationId)) != null)
                    continue;

                var invitation = invitationService.GetInvitation(invitationId);
                if (!requestId.HasValue && requestStartedAtUtc.HasValue &&
                    invitation.LastReminderSentOnUtc >= requestStartedAtUtc.Value)
                    continue;
                if (string.IsNullOrWhiteSpace(invitation.Assignment.Email))
                    continue;

                var sent = await SendReminderAsync(invitation, questionnaireTitle, emailTemplate, senderInfo,
                    reminderType == EmailTextTemplateType.RejectEmail, requestId).ConfigureAwait(false);
                if (sent)
                    result.SentCount++;
                else
                    result.FailedCount++;
            }

            return result;
        }

        private async Task<bool> SendReminderAsync(Invitation invitation, string questionnaireTitle,
            WebInterviewEmailTemplate emailTemplate, ISenderInformation senderInfo, bool rejected, Guid? requestId)
        {
            var password = invitation.Assignment.Password;
            var address = invitation.Assignment.Email;
            var link = rejected
                ? interviewLinkProvider.WebInterviewContinueLink(invitation)
                : interviewLinkProvider.WebInterviewStartLink(invitation);
            var emailContent = new EmailContent(emailTemplate, questionnaireTitle, link, password);
            var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitation.Id}" + (rejected ? "-Reject" : "");
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
            var interviewEmail = await webInterviewEmailRenderer.RenderEmail(emailParams).ConfigureAwait(false);
            var sent = false;
            await inScopeExecutor.ExecuteAsync(async locator =>
            {
                var localInvitations = locator.GetInstance<IInvitationService>();
                locator.GetInstance<IPlainKeyValueStorage<EmailParameters>>().Store(emailParams, emailParamsId);

                string emailId = null;
                try
                {
                    emailId = await emailService.SendEmailAsync(address, emailParams.Subject,
                        interviewEmail.MessageHtml, interviewEmail.MessageText).ConfigureAwait(false);
                    sent = true;
                }
                catch (Exception exception)
                {
                    if (rejected)
                        localInvitations.RejectedInterviewReminderWasNotSent(invitation.Id);
                    else
                        localInvitations.ReminderWasNotSent(invitation.Id, invitation.AssignmentId, address,
                            exception.Message);
                    logger.LogError(exception, "Interview email was not sent for invitation {InvitationId}.", invitation.Id);
                }

                if (sent)
                {
                    if (rejected)
                        localInvitations.MarkRejectedInterviewReminderSent(invitation.Id, emailId);
                    else
                        localInvitations.MarkInvitationAsReminded(invitation.Id, emailId);
                }

                if (requestId.HasValue)
                {
                    var storage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>();
                    var key = ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value);
                    var outcomes = storage.GetByIdUncached(key) ?? new ReminderEmailDeliveryOutcomes();
                    if (sent)
                        outcomes.SentCount++;
                    else
                        outcomes.FailedCount++;
                    storage.Store(outcomes, key);
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>()
                        .Store(new ReminderEmailDeliveryCheckpoint { Sent = sent },
                            ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId.Value, invitation.Id));
                }
            }).ConfigureAwait(false);
            return sent;
        }
    }
}
