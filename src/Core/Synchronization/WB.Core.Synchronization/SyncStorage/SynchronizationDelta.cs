using System;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SynchronizationDelta : IView
    {
        public SynchronizationDelta()
        {
        }


        public SynchronizationDelta(Guid publicKey, string content, long sequence, Guid userId)
            : this(publicKey, content, sequence, userId, false, "temp")
        {
        }

        public SynchronizationDelta(Guid publicKey, string content, long sequence, Guid userId, bool isCompressed, string itemType)
        {
            PublicKey = publicKey;
            Content = content;
            Sequence = sequence;
            UserId = userId;
            IsCompressed = isCompressed;
            ItemType = itemType;
        }

        public Guid PublicKey { get; private set; }
        public string Content { get; private set; }
        public long Sequence { get; private set; }
        public Guid UserId { get; private set; }
        public bool IsCompressed { get; private set; }
        public string ItemType { get; private set; }
    }
}