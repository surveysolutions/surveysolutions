﻿using System;
using System.Linq;
using Raven.Client.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    public class SynchronizationDeltasByBriefFields :
        AbstractIndexCreationTask<SynchronizationDelta, SynchronizationDeltasByBriefFields.SyncPackageBrief>
    {
        public class SyncPackageBrief
        {
            public string ItemType { get; set; }
            public DateTime Timestamp { get; set; }
            public Guid UserId { get; set; }
            public string PublicKey { get; set; }
            public int SortIndex { get; set; }
        }

        public SynchronizationDeltasByBriefFields()
        {
            this.Map = interviews => from doc in interviews
                                     select new SyncPackageBrief { PublicKey = doc.PublicKey, ItemType = doc.ItemType, Timestamp = doc.Timestamp, UserId = doc.UserId, SortIndex = doc.SortIndex };
        }
    }
}
