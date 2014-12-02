using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SynchronizationDelta : IView
    {
        [Obsolete("Probably used for deserialization")]
        public SynchronizationDelta()
        {
        }

        public SynchronizationDelta(Guid publicKey, 
            string content, 
            DateTime timestamp, 
            Guid? userId, 
            bool isCompressed, 
            string itemType, 
            string metaInfo,
            int sortIndex)
        {
            this.RootId = publicKey;
            this.PublicKey = publicKey.FormatGuid() + "$" + sortIndex;
            this.Content = content;
            this.Timestamp = timestamp;
            this.UserId = userId ?? Guid.Empty;
            this.IsCompressed = isCompressed;
            this.ItemType = itemType;
            this.MetaInfo = metaInfo;
            this.SortIndex = sortIndex;
        }
        public Guid RootId { get; private set; }
        public string PublicKey { get; private set; }
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; }
        public Guid UserId { get; private set; }
        public bool IsCompressed { get; private set; }
        public string ItemType { get; private set; }
        public string MetaInfo { get; private set; }
        public int SortIndex { get; private set; }
    }
}