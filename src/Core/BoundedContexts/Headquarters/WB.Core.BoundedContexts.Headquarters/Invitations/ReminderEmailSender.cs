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
            string questionnaireTitle, EmailTextTemplateType reminderType, int thresholdDays, Guid? requestId = null)
        {
            if (!await ReminderEmailSendLock.TryAcquireDeliveryAsync(unitOfWork, questionnaireIdentity, reminderType)
                    .ConfigureAwait(false))
                throw new ReminderDeliveryInProgressException();

            var outcomes = requestId.HasValue
                ? deliveryOutcomes.GetByIdUncached(ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value))
                    ?? new ReminderEmailDeliveryOutcomes()
                : new ReminderEmailDeliveryOutcomes();
            var result = outcomes.GetResult();
            if (requestId.HasValue && outcomes.InvitationIds != null)
            {
                foreach (var invitationId in outcomes.InvitationIds)
                {
                    var checkpoint = deliveryCheckpoints.GetByIdUncached(
                        ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId.Value, invitationId));
                    if (checkpoint?.AttemptInProgress == true &&
                        await MarkDeliveryUnconfirmedAsync(requestId.Value, invitationId).ConfigureAwait(false))
                        result.UnconfirmedCount++;
                }
            }

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
                // Request checkpoints retain deliveries and recipients that no longer match the live query after a restart.
                if (requestId.HasValue && deliveryCheckpoints.GetByIdUncached(
                        ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId.Value, invitationId)) != null)
                    continue;

                var invitation = invitationService.GetInvitation(invitationId);
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

            if (requestId.HasValue)
            {
                await inScopeExecutor.ExecuteAsync(locator =>
                {
                    locator.GetInstance<IPlainKeyValueStorage<EmailParameters>>().Store(emailParams, emailParamsId);
                    var outcomesStorage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>();
                    var outcomesKey = ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value);
                    var outcomes = outcomesStorage.GetByIdUncached(outcomesKey) ?? new ReminderEmailDeliveryOutcomes();
                    outcomes.InvitationIds ??= new List<int>();
                    if (!outcomes.InvitationIds.Contains(invitation.Id))
                        outcomes.InvitationIds.Add(invitation.Id);
                    outcomesStorage.Store(outcomes, outcomesKey);
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>()
                        .Store(new ReminderEmailDeliveryCheckpoint { AttemptInProgress = true },
                            ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId.Value, invitation.Id));
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
            }

            var sent = false;
            await inScopeExecutor.ExecuteAsync(async locator =>
            {
                var localInvitations = locator.GetInstance<IInvitationService>();
                if (!requestId.HasValue)
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

        private async Task<bool> MarkDeliveryUnconfirmedAsync(Guid requestId, int invitationId)
        {
            return await inScopeExecutor.ExecuteAsync(locator =>
            {
                var checkpointStorage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>();
                var checkpointKey = ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId, invitationId);
                var checkpoint = checkpointStorage.GetByIdUncached(checkpointKey);
                if (checkpoint?.AttemptInProgress != true)
                    return Task.FromResult(false);

                checkpoint.AttemptInProgress = false;
                checkpoint.Unconfirmed = true;
                checkpointStorage.Store(checkpoint, checkpointKey);

                var outcomesStorage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>();
                var outcomesKey = ReminderEmailDeliveryOutcomes.GetStorageKey(requestId);
                var outcomes = outcomesStorage.GetByIdUncached(outcomesKey) ?? new ReminderEmailDeliveryOutcomes();
                outcomes.InvitationIds ??= new List<int>();
                outcomes.UnconfirmedCount++;
                outcomesStorage.Store(outcomes, outcomesKey);
                return Task.FromResult(true);
            }).ConfigureAwait(false);
        }
    }
}
