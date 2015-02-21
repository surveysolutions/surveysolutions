using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SynchronizationDeltaMetaInformation : IView
    {
        [Obsolete("Probably used for deserialization")]
        public SynchronizationDeltaMetaInformation()
        {
        }

        public SynchronizationDeltaMetaInformation(Guid publicKey, 
            DateTime timestamp,
            Guid? userId, string itemType, 
            int sortIndex, int contentLength, int metaDataLength)
        {
            this.RootId = publicKey;
            this.PublicKey = publicKey.FormatGuid() + "$" + sortIndex;
            this.Timestamp = timestamp;
            this.UserId = userId ?? Guid.Empty;
            this.SortIndex = sortIndex;
            this.ItemType = itemType;
            this.ContentLength = contentLength;
            this.MetaDataLength = metaDataLength;
        }
        public Guid RootId { get; private set; }
        public string PublicKey { get; private set; }
        public DateTime Timestamp { get; private set; }
        public Guid UserId { get; private set; }
        public int SortIndex { get; private set; }
        public string ItemType { get; set; }
        public int ContentLength { get; private set; }
        public int MetaDataLength { get; private set; }
    }
}