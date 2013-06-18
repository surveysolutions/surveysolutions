using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItemsMetaContainer
    {
        public SyncItemsMetaContainer()
        {
            ARId = new List<Guid>();
        }

        public List<Guid> ARId { set; get; }
    }

}