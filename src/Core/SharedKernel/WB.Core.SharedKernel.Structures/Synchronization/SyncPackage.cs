using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    
    public class SyncPackage
    {
        public Guid Id;

        public Guid SyncProcessKey;

        public List<SyncItem> ItemsContainer;

        public bool IsErrorOccured;
        public string ErrorMessage;

        public SyncPackage()
        {
            ItemsContainer = new List<SyncItem>();
            Id = Guid.NewGuid();
        }
    }
}
