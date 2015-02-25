using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    public class InterviewSyncPackagesByBriefFields : AbstractIndexCreationTask<InterviewSyncPackageMeta, InterviewSyncPackagesByBriefFields.SyncPackageBrief>
    {
        public class SyncPackageBrief
        {
            public string PackageId { get; set; }

            public Guid InterviewId { get; set; }

            public Guid UserId { get; set; }

            public long SortIndex { get; set; }

            public long ContentSize { get; set; }
        }

        public InterviewSyncPackagesByBriefFields()
        {
            this.Map = interviews => from doc in interviews
                select new SyncPackageBrief
                       {
                           PackageId = doc.PackageId,
                           InterviewId = doc.InterviewId,
                           UserId = doc.UserId,
                           SortIndex = doc.SortIndex,
                           ContentSize = doc.ContentSize
                       };

            this.Index(x => x.SortIndex, FieldIndexing.Default);
            this.Sort(x => x.SortIndex, SortOptions.Long);
        }
    }
}