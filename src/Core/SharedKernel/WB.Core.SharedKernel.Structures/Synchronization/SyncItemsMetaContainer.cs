using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItemsMetaContainer
    {
        public SyncItemsMetaContainer()
        {
            ARId = new List<SyncItemsMeta>();
        }

        public List<SyncItemsMeta> ARId { set; get; }
    }

}