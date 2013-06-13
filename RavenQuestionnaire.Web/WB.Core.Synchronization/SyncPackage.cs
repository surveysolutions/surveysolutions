using System;
using System.Collections.Generic;

namespace WB.Core.Synchronization
{
    public class SyncPackage
    {
        public Guid Id;

        public Guid SyncProcessKey;

        public List<SyncItem> ItemsContainer;

        public bool Status;

        public string Message;

        public SyncPackage()
        {
            ItemsContainer = new List<SyncItem>();
            Id = Guid.NewGuid();
        }
    }
}
