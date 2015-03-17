using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    [Obsolete("Removed")]
    public class QuestionnaireSyncPackagesByBriefFields : AbstractIndexCreationTask<QuestionnaireSyncPackageMeta, QuestionnaireSyncPackagesByBriefFields.SyncPackageBrief>
    {
        public class SyncPackageBrief
        {
            public string PackageId { get; set; }

            public long SortIndex { get; set; }
        }

        public QuestionnaireSyncPackagesByBriefFields()
        {
            this.Map = interviews => from doc in interviews
                select new SyncPackageBrief
                       {
                           PackageId = doc.PackageId,
                           SortIndex = doc.SortIndex
                       };

            this.Index(x => x.SortIndex, FieldIndexing.Default);
            this.Sort(x => x.SortIndex, SortOptions.Long);
        }
    }
}