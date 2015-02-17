using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SynchronizationDeltaContent : IView
    {
        [Obsolete("Probably used for deserialization")]
        public SynchronizationDeltaContent()
        {
        }

        public SynchronizationDeltaContent(string id, string content, string metaInfo, bool isCompressed, string itemType, Guid rootId)
        {
            Id = id;
            Content = content;
            MetaInfo = metaInfo;
            IsCompressed = isCompressed;
            ItemType = itemType;
            RootId = rootId;
        }

        public string Id { get;  set; }
        public string Content { get;  set; }
        public string MetaInfo { get;  set; }
        public bool IsCompressed { get;  set; }
        public string ItemType { get;  set; }
        public Guid RootId { get;  set; }
    }
}
