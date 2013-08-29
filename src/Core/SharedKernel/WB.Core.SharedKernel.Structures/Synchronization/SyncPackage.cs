using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{

    public class SyncPackage : BasePackage
    {
        public Guid Id;
        public Guid SyncProcessKey;
        public List<SyncItem> ItemsContainer;

        public SyncPackage()
        {
            ItemsContainer = new List<SyncItem>();
            Id = Guid.NewGuid();
        }
    }
}
