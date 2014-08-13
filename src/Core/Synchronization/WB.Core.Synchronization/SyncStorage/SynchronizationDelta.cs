using System;
using Main.Core;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SynchronizationDelta : IView
    {
        public SynchronizationDelta()
        {
        }


        public SynchronizationDelta(Guid publicKey, string content, DateTime timestamp, Guid userId)
            : this(publicKey, content, timestamp, userId, false, "temp", "")
        {
        }

        public SynchronizationDelta(Guid publicKey, string content, DateTime timestamp, Guid? userId, bool isCompressed, string itemType, string metaInfo)
        {
            PublicKey = publicKey;
            Content = isCompressed ? PackageHelper.CompressString(content) : content;
            Timestamp = timestamp;
            UserId = userId;
            IsCompressed = isCompressed;
            ItemType = itemType;
            MetaInfo = (isCompressed && !string.IsNullOrWhiteSpace(metaInfo)) ? PackageHelper.CompressString(metaInfo) : metaInfo; 
        }

        public Guid PublicKey { get; private set; }
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; }
        public Guid? UserId { get; private set; }
        public bool IsCompressed { get; private set; }
        public string ItemType { get; private set; }

        public string MetaInfo { get; private set; }
    }
}