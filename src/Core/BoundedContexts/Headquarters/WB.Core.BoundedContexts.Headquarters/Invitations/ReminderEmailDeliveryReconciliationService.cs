using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class ReminderEmailUnresolvedDeliveryView
    {
        public int InvitationId { get; set; }
        public WB.Core.BoundedContexts.Headquarters.WebInterview.EmailTextTemplateType Type { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class ReminderEmailUnresolvedDeliveryPage
    {
        public int TotalCount { get; set; }
        public List<ReminderEmailUnresolvedDeliveryView> Items { get; set; } =
            new List<ReminderEmailUnresolvedDeliveryView>();
    }

    public class ReminderDeliveryReconciliationInProgressException : Exception
    {
        public ReminderDeliveryReconciliationInProgressException()
            : base("An active reminder request cannot be reconciled.")
        {
        }
    }

    public interface IReminderEmailDeliveryReconciliationService
    {
        Task<ReminderEmailUnresolvedDeliveryPage> GetUnresolvedAsync(QuestionnaireIdentity questionnaireIdentity,
            int offset, int limit);
        Task<bool> ReconcileAsync(QuestionnaireIdentity questionnaireIdentity, int invitationId, bool delivered);
    }

    public class ReminderEmailDeliveryReconciliationService : IReminderEmailDeliveryReconciliationService
    {
        private readonly IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery> deliveries;
        private readonly IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex> indexes;
        private readonly IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk> chunks;
        private readonly IPlainKeyValueStorage<SendRemindersNowStatus> requestStatuses;
        private readonly IInvitationService invitationService;
        private readonly IUnitOfWork unitOfWork;

        public ReminderEmailDeliveryReconciliationService(
            IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery> deliveries,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex> indexes,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk> chunks,
            IPlainKeyValueStorage<SendRemindersNowStatus> requestStatuses,
            IInvitationService invitationService,
            IUnitOfWork unitOfWork)
        {
            this.deliveries = deliveries;
            this.indexes = indexes;
            this.chunks = chunks;
            this.requestStatuses = requestStatuses;
            this.invitationService = invitationService;
            this.unitOfWork = unitOfWork;
        }

        public Task<ReminderEmailUnresolvedDeliveryPage> GetUnresolvedAsync(
            QuestionnaireIdentity questionnaireIdentity, int offset, int limit)
        {
            var index = indexes.GetByIdUncached(
                ReminderEmailUnresolvedDeliveryIndex.GetStorageKey(questionnaireIdentity));
            var page = new ReminderEmailUnresolvedDeliveryPage { TotalCount = index?.Count ?? 0 };
            if (index == null || limit == 0)
                return Task.FromResult(page);

            var skipped = 0;
            foreach (var invitationId in GetInvitationIds(questionnaireIdentity, index))
            {
                var delivery = deliveries.GetByIdUncached(
                    ReminderEmailUnresolvedDelivery.GetStorageKey(questionnaireIdentity, invitationId));
                if (delivery == null)
                    continue;

                if (skipped++ < offset)
                    continue;

                page.Items.Add(new ReminderEmailUnresolvedDeliveryView
                {
                    InvitationId = invitationId,
                    Type = delivery.Type,
                    CreatedAtUtc = delivery.CreatedAtUtc
                });
                if (page.Items.Count == limit)
                    break;
            }

            return Task.FromResult(page);
        }

        public async Task<bool> ReconcileAsync(QuestionnaireIdentity questionnaireIdentity, int invitationId,
            bool delivered)
        {
            if (!await ReminderEmailSendLock.TryAcquireDeliveryAsync(unitOfWork, questionnaireIdentity)
                    .ConfigureAwait(false))
                throw new ReminderDeliveryInProgressException();

            var claim = deliveries.GetByIdUncached(
                ReminderEmailUnresolvedDelivery.GetStorageKey(questionnaireIdentity, invitationId));
            if (claim == null)
                return false;

            if (claim.RequestId.HasValue)
            {
                var requestStatus = requestStatuses.GetByIdUncached(
                    SendRemindersNowRequest.GetStatusStorageKey(claim.RequestId.Value));
                if (requestStatus?.State is SendRemindersNowState.Queued or SendRemindersNowState.InProgress)
                    throw new ReminderDeliveryReconciliationInProgressException();
            }

            if (delivered)
            {
                if (invitationService.GetInvitation(invitationId) == null)
                    return false;

                if (claim.Type == WB.Core.BoundedContexts.Headquarters.WebInterview.EmailTextTemplateType.RejectEmail)
                    invitationService.MarkRejectedInterviewReminderSent(invitationId, string.Empty);
                else
                    invitationService.MarkInvitationAsReminded(invitationId, string.Empty);
            }

            ReminderEmailUnresolvedDeliveryStorage.Remove(deliveries, indexes, chunks, questionnaireIdentity,
                invitationId);
            return true;
        }

        private IEnumerable<int> GetInvitationIds(QuestionnaireIdentity questionnaireIdentity,
            ReminderEmailUnresolvedDeliveryIndex index)
        {
            for (var chunkIndex = 0; chunkIndex < index.ChunkCount; chunkIndex++)
            {
                var chunk = chunks.GetByIdUncached(
                    ReminderEmailUnresolvedDeliveryChunk.GetStorageKey(questionnaireIdentity, chunkIndex));
                if (chunk?.InvitationIds == null)
                    continue;

                foreach (var invitationId in chunk.InvitationIds)
                    yield return invitationId;
            }
        }
    }
}
