using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public static class ReminderEmailUnresolvedDeliveryStorage
    {
        public static void Store(
            IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery> deliveries,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex> indexes,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk> chunks,
            QuestionnaireIdentity questionnaireIdentity,
            int invitationId,
            EmailTextTemplateType type,
            Guid? requestId)
        {
            var claimKey = ReminderEmailUnresolvedDelivery.GetStorageKey(questionnaireIdentity, invitationId);
            var claim = deliveries.GetByIdUncached(claimKey);
            if (claim?.IndexChunk >= 0)
                return;

            var indexKey = ReminderEmailUnresolvedDeliveryIndex.GetStorageKey(questionnaireIdentity);
            var index = indexes.GetByIdUncached(indexKey) ?? new ReminderEmailUnresolvedDeliveryIndex();
            var chunkIndex = Math.Max(0, index.ChunkCount - 1);
            var chunkKey = ReminderEmailUnresolvedDeliveryChunk.GetStorageKey(questionnaireIdentity, chunkIndex);
            var chunk = chunks.GetByIdUncached(chunkKey) ?? new ReminderEmailUnresolvedDeliveryChunk();
            if (chunk.InvitationIds.Count >= ReminderEmailUnresolvedDeliveryIndex.InvitationChunkSize)
            {
                chunkIndex = index.ChunkCount;
                chunkKey = ReminderEmailUnresolvedDeliveryChunk.GetStorageKey(questionnaireIdentity, chunkIndex);
                chunk = new ReminderEmailUnresolvedDeliveryChunk();
            }

            chunk.InvitationIds.Add(invitationId);
            chunks.Store(chunk, chunkKey);

            claim ??= new ReminderEmailUnresolvedDelivery
            {
                Type = type,
                RequestId = requestId,
                CreatedAtUtc = DateTime.UtcNow
            };
            claim.IndexChunk = chunkIndex;
            deliveries.Store(claim, claimKey);

            index.Count++;
            index.ChunkCount = Math.Max(index.ChunkCount, chunkIndex + 1);
            if (claim.RequestId.HasValue)
            {
                index.RequestCounts ??= new System.Collections.Generic.Dictionary<Guid, int>();
                index.RequestCounts.TryGetValue(claim.RequestId.Value, out var requestCount);
                index.RequestCounts[claim.RequestId.Value] = requestCount + 1;
            }

            indexes.Store(index, indexKey);
        }

        public static ReminderEmailUnresolvedDelivery Remove(
            IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery> deliveries,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex> indexes,
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryChunk> chunks,
            QuestionnaireIdentity questionnaireIdentity,
            int invitationId)
        {
            var claimKey = ReminderEmailUnresolvedDelivery.GetStorageKey(questionnaireIdentity, invitationId);
            var claim = deliveries.GetByIdUncached(claimKey);
            if (claim == null)
                return null;

            var indexKey = ReminderEmailUnresolvedDeliveryIndex.GetStorageKey(questionnaireIdentity);
            var index = indexes.GetByIdUncached(indexKey);
            if (index != null)
            {
                var chunkIndex = claim.IndexChunk;
                if (chunkIndex < 0)
                {
                    for (var candidate = 0; candidate < index.ChunkCount; candidate++)
                    {
                        var candidateKey = ReminderEmailUnresolvedDeliveryChunk.GetStorageKey(questionnaireIdentity,
                            candidate);
                        var candidateChunk = chunks.GetByIdUncached(candidateKey);
                        if (candidateChunk?.InvitationIds.Contains(invitationId) == true)
                        {
                            chunkIndex = candidate;
                            break;
                        }
                    }
                }

                if (chunkIndex >= 0)
                {
                    var chunkKey = ReminderEmailUnresolvedDeliveryChunk.GetStorageKey(questionnaireIdentity,
                        chunkIndex);
                    var chunk = chunks.GetByIdUncached(chunkKey);
                    if (chunk?.InvitationIds.Remove(invitationId) == true)
                    {
                        if (chunk.InvitationIds.Count == 0)
                            chunks.Remove(chunkKey);
                        else
                            chunks.Store(chunk, chunkKey);
                        index.Count = Math.Max(0, index.Count - 1);
                        if (claim.RequestId.HasValue)
                        {
                            index.RequestCounts ??= new System.Collections.Generic.Dictionary<Guid, int>();
                            if (index.RequestCounts.TryGetValue(claim.RequestId.Value, out var requestCount))
                            {
                                if (requestCount <= 1)
                                    index.RequestCounts.Remove(claim.RequestId.Value);
                                else
                                    index.RequestCounts[claim.RequestId.Value] = requestCount - 1;
                            }
                        }

                        while (index.ChunkCount > 0)
                        {
                            var lastChunkIndex = index.ChunkCount - 1;
                            var lastChunkKey = ReminderEmailUnresolvedDeliveryChunk.GetStorageKey(questionnaireIdentity,
                                lastChunkIndex);
                            var lastChunk = chunks.GetByIdUncached(lastChunkKey);
                            if (lastChunk?.InvitationIds.Count > 0)
                                break;
                            chunks.Remove(lastChunkKey);
                            index.ChunkCount--;
                        }
                    }
                }

                if (index.Count == 0)
                    indexes.Remove(indexKey);
                else
                    indexes.Store(index, indexKey);
            }

            deliveries.Remove(claimKey);
            return claim;
        }

        public static int GetCount(
            IPlainKeyValueStorage<ReminderEmailUnresolvedDeliveryIndex> indexes,
            QuestionnaireIdentity questionnaireIdentity,
            Guid? requestId = null)
        {
            var index = indexes.GetByIdUncached(
                ReminderEmailUnresolvedDeliveryIndex.GetStorageKey(questionnaireIdentity));
            if (index == null)
                return 0;

            var count = index.Count;
            if (requestId.HasValue && index.RequestCounts != null &&
                index.RequestCounts.TryGetValue(requestId.Value, out var requestCount))
                count -= requestCount;

            return Math.Max(0, count);
        }
    }
}
