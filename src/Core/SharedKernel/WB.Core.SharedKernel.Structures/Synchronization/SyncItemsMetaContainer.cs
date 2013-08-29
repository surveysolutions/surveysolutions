using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItemsMetaContainer : BasePackage
    {
        public SyncItemsMetaContainer()
        {
            ARId = new List<KeyValuePair<long, Guid>>();
        }

        public List<KeyValuePair<long,Guid>> ARId { set; get; }

    }

}