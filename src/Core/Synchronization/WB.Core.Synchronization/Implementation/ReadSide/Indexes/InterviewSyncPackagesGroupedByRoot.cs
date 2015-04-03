using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    public class InterviewSyncPackagesGroupedByRoot :
        AbstractIndexCreationTask<InterviewSyncPackageMeta, InterviewSyncPackagesGroupedByRoot.SyncPackageGroup>
    {
        public InterviewSyncPackagesGroupedByRoot()
        {
            this.Map = interviews => from doc in interviews
                select new SyncPackageGroup
                {
                    PackageId = doc.PackageId,
                    InterviewId = doc.InterviewId,
                    UserId = doc.UserId,
                    SortIndex = doc.SortIndex,
                    ContentSize = doc.ContentSize,
                    ItemType = doc.ItemType
                };

            this.Reduce = results => from result in results
                group result by new { result.InterviewId, result.UserId }
                into g
                let lastPackage = g.OrderBy(x => x.SortIndex).Last()
                select new SyncPackageGroup
                {
                    PackageId = lastPackage.PackageId,
                    InterviewId = lastPackage.InterviewId,
                    UserId = lastPackage.UserId,
                    SortIndex = lastPackage.SortIndex,
                    ContentSize = lastPackage.ContentSize,
                    ItemType = lastPackage.ItemType
                };

            this.Index(x => x.SortIndex, FieldIndexing.Default);
            this.Sort(x => x.SortIndex, SortOptions.Long);
        }

        public class SyncPackageGroup
        {
            public string PackageId { get; set; }

            public Guid InterviewId { get; set; }

            public Guid UserId { get; set; }

            public long SortIndex { get; set; }

            public long ContentSize { get; set; }

            public string ItemType { get; set; }
        }
    }
}
