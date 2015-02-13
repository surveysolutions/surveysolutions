﻿using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    public class UserSyncPackagesByBriefFields : AbstractIndexCreationTask<UserSyncPackage, UserSyncPackagesByBriefFields.SyncPackageBrief>
    {
        public class SyncPackageBrief
        {
            public string PackageId { get; set; }

            public DateTime Timestamp { get; set; }

            public Guid UserId { get; set; }

            public long SortIndex { get; set; }
        }

        public UserSyncPackagesByBriefFields()
        {
            Map = interviews => from doc in interviews
                                     select new SyncPackageBrief {
                                         PackageId = doc.PackageId, 
                                         Timestamp = doc.Timestamp, 
                                         UserId = doc.UserId,
                                         SortIndex = doc.SortIndex 
                                     };

            Index(x => x.SortIndex, FieldIndexing.Default);
            Sort(x => x.SortIndex, SortOptions.Long);
        }
    }
}
