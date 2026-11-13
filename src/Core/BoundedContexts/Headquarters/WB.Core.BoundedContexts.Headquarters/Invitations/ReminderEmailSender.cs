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
        private readonly IPlainKeyValueStorage<ReminderEmailDeliveryInvitationChunk> deliveryInvitationChunks;
        private readonly IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint> deliveryCheckpoints;
        private readonly IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery> unresolvedDeliveries;
        private readonly IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex> unresolvedDeliveryIndexes;
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
            IPlainKeyValueStorage<ReminderEmailDeliveryInvitationChunk> deliveryInvitationChunks,
            IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint> deliveryCheckpoints,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery> unresolvedDeliveries,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex> unresolvedDeliveryIndexes,
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
            this.deliveryInvitationChunks = deliveryInvitationChunks;
            this.deliveryCheckpoints = deliveryCheckpoints;
            this.unresolvedDeliveries = unresolvedDeliveries;
            this.unresolvedDeliveryIndexes = unresolvedDeliveryIndexes;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
            this.inScopeExecutor = inScopeExecutor;
            this.interviewLinkProvider = interviewLinkProvider;
            this.unitOfWork = unitOfWork;
        }

        public async Task<ReminderEmailSendResult> SendRemindersAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, EmailTextTemplateType reminderType, int thresholdDays, Guid? requestId = null)
        {
            if (!await ReminderEmailSendLock.TryAcquireDeliveryAsync(unitOfWork, questionnaireIdentity)
                    .ConfigureAwait(false))
                throw new ReminderDeliveryInProgressException();

            var outcomes = requestId.HasValue
                ? deliveryOutcomes.GetByIdUncached(ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value))
                    ?? new ReminderEmailDeliveryOutcomes()
                : new ReminderEmailDeliveryOutcomes();
            if (requestId.HasValue && outcomes.InFlightInvitationId.HasValue &&
                await MarkDeliveryUnconfirmedAsync(requestId.Value, questionnaireIdentity,
                    outcomes.InFlightInvitationId.Value, reminderType).ConfigureAwait(false))
            {
                outcomes = deliveryOutcomes.GetByIdUncached(
                    ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value)) ?? outcomes;
            }

            var result = outcomes.GetResult();
            result.UnconfirmedCount += ReminderEmailUnresolvedDeliveryStorage.GetCount(
                unresolvedDeliveryIndexes, questionnaireIdentity, requestId);
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

                if (unresolvedDeliveries.GetByIdUncached(
                        ReminderEmailUnresolvedDelivery.GetStorageKey(questionnaireIdentity, invitationId)) != null)
                    continue;

                var invitation = invitationService.GetInvitation(invitationId);
                if (string.IsNullOrWhiteSpace(invitation.Assignment.Email))
                    continue;

                var deliveryResult = await SendReminderAsync(invitation, questionnaireIdentity, questionnaireTitle,
                    emailTemplate, senderInfo, reminderType, requestId).ConfigureAwait(false);
                switch (deliveryResult)
                {
                    case ReminderDeliveryResult.Sent:
                        result.SentCount++;
                        break;
                    case ReminderDeliveryResult.Failed:
                        result.FailedCount++;
                        break;
                    case ReminderDeliveryResult.Unconfirmed:
                        result.UnconfirmedCount++;
                        break;
                }
            }

            return result;
        }

        private async Task<ReminderDeliveryResult> SendReminderAsync(Invitation invitation,
            QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle,
            WebInterviewEmailTemplate emailTemplate, ISenderInformation senderInfo,
            EmailTextTemplateType reminderType, Guid? requestId)
        {
            var rejected = reminderType == EmailTextTemplateType.RejectEmail;
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

            await inScopeExecutor.ExecuteAsync(locator =>
            {
                locator.GetInstance<IPlainKeyValueStorage<EmailParameters>>().Store(emailParams, emailParamsId);
                ReminderEmailUnresolvedDeliveryStorage.Store(
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>(),
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex>>(),
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk>>(),
                    questionnaireIdentity, invitation.Id, reminderType, requestId);

                if (requestId.HasValue)
                {
                    var outcomesStorage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>();
                    var outcomesKey = ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value);
                    var outcomes = outcomesStorage.GetByIdUncached(outcomesKey) ?? new ReminderEmailDeliveryOutcomes();

                    var chunkIndex = outcomes.InvitationCount / ReminderEmailDeliveryOutcomes.InvitationChunkSize;
                    var chunkKey = ReminderEmailDeliveryInvitationChunk.GetStorageKey(requestId.Value, chunkIndex);
                    var chunksStorage =
                        locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryInvitationChunk>>();
                    var chunk = chunksStorage.GetByIdUncached(chunkKey) ?? new ReminderEmailDeliveryInvitationChunk();
                    if (!chunk.InvitationIds.Contains(invitation.Id))
                    {
                        chunk.InvitationIds.Add(invitation.Id);
                        chunksStorage.Store(chunk, chunkKey);
                        outcomes.InvitationCount++;
                        outcomes.InvitationChunkCount = Math.Max(outcomes.InvitationChunkCount, chunkIndex + 1);
                        outcomesStorage.Store(outcomes, outcomesKey);
                    }
                    outcomes.InFlightInvitationId = invitation.Id;
                    outcomesStorage.Store(outcomes, outcomesKey);

                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>()
                        .Store(new ReminderEmailDeliveryCheckpoint { AttemptInProgress = true },
                            ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId.Value, invitation.Id));
                }

                return Task.CompletedTask;
            }).ConfigureAwait(false);

            var deliveryResult = ReminderDeliveryResult.Sent;
            await inScopeExecutor.ExecuteAsync(async locator =>
            {
                var localInvitations = locator.GetInstance<IInvitationService>();

                string emailId = null;
                try
                {
                    emailId = await emailService.SendEmailAsync(address, emailParams.Subject,
                        interviewEmail.MessageHtml, interviewEmail.MessageText).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    if (IsAmbiguousDeliveryFailure(exception))
                    {
                        deliveryResult = ReminderDeliveryResult.Unconfirmed;
                    }
                    else
                    {
                        deliveryResult = ReminderDeliveryResult.Failed;
                        if (rejected)
                            localInvitations.RejectedInterviewReminderWasNotSent(invitation.Id);
                        else
                            localInvitations.ReminderWasNotSent(invitation.Id, invitation.AssignmentId, address,
                                exception.Message);
                        ReminderEmailUnresolvedDeliveryStorage.Remove(
                            locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>(),
                            locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex>>(),
                            locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk>>(),
                            questionnaireIdentity, invitation.Id);
                    }
                    logger.LogError(exception, "Interview email delivery failed or could not be confirmed for invitation {InvitationId}.",
                        invitation.Id);
                }

                if (deliveryResult == ReminderDeliveryResult.Sent)
                {
                    if (rejected)
                        localInvitations.MarkRejectedInterviewReminderSent(invitation.Id, emailId);
                    else
                        localInvitations.MarkInvitationAsReminded(invitation.Id, emailId);
                    ReminderEmailUnresolvedDeliveryStorage.Remove(
                        locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>(),
                        locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex>>(),
                        locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk>>(),
                        questionnaireIdentity, invitation.Id);
                }

                if (requestId.HasValue)
                {
                    var storage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>();
                    var key = ReminderEmailDeliveryOutcomes.GetStorageKey(requestId.Value);
                    var outcomes = storage.GetByIdUncached(key) ?? new ReminderEmailDeliveryOutcomes();
                    switch (deliveryResult)
                    {
                        case ReminderDeliveryResult.Sent:
                            outcomes.SentCount++;
                            break;
                        case ReminderDeliveryResult.Failed:
                            outcomes.FailedCount++;
                            break;
                        case ReminderDeliveryResult.Unconfirmed:
                            outcomes.UnconfirmedCount++;
                            break;
                    }
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>()
                        .Store(new ReminderEmailDeliveryCheckpoint
                        {
                            Sent = deliveryResult == ReminderDeliveryResult.Sent,
                            Unconfirmed = deliveryResult == ReminderDeliveryResult.Unconfirmed
                        },
                            ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId.Value, invitation.Id));
                    outcomes.InFlightInvitationId = null;
                    storage.Store(outcomes, key);
                }
            }).ConfigureAwait(false);
            return deliveryResult;
        }

        private static bool IsAmbiguousDeliveryFailure(Exception exception)
        {
            if (exception is not EmailServiceException emailException)
                return true;

            return emailException.DeliveryIsAmbiguous;
        }

        private async Task<bool> MarkDeliveryUnconfirmedAsync(Guid requestId,
            QuestionnaireIdentity questionnaireIdentity, int invitationId, EmailTextTemplateType reminderType)
        {
            return await inScopeExecutor.ExecuteAsync(locator =>
            {
                var checkpointStorage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>();
                var checkpointKey = ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId, invitationId);
                var checkpoint = checkpointStorage.GetByIdUncached(checkpointKey) ??
                                 new ReminderEmailDeliveryCheckpoint();
                checkpoint.AttemptInProgress = false;
                checkpoint.Unconfirmed = true;
                checkpointStorage.Store(checkpoint, checkpointKey);
                ReminderEmailUnresolvedDeliveryStorage.Store(
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>(),
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex>>(),
                    locator.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk>>(),
                    questionnaireIdentity, invitationId, reminderType, requestId);

                var outcomesStorage = locator.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>();
                var outcomesKey = ReminderEmailDeliveryOutcomes.GetStorageKey(requestId);
                var outcomes = outcomesStorage.GetByIdUncached(outcomesKey) ?? new ReminderEmailDeliveryOutcomes();
                outcomes.UnconfirmedCount++;
                outcomes.InFlightInvitationId = null;
                outcomesStorage.Store(outcomes, outcomesKey);
                return Task.FromResult(true);
            }).ConfigureAwait(false);
        }

        private enum ReminderDeliveryResult
        {
            Sent,
            Failed,
            Unconfirmed
        }
    }
}
