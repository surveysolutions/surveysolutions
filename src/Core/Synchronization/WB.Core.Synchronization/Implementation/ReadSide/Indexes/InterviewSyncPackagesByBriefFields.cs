using System;
using System.Linq;

using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    public class InterviewSyncPackagesByBriefFields : AbstractIndexCreationTask<InterviewSyncPackage, InterviewSyncPackagesByBriefFields.SyncPackageBrief>
    {
        public class SyncPackageBrief
        {
            public string PackageId { get; set; }

            public DateTime Timestamp { get; set; }

            public Guid UserId { get; set; }

            public long SortIndex { get; set; }
        }

        public InterviewSyncPackagesByBriefFields()
        {
            this.Map = interviews => from doc in interviews
                select new SyncPackageBrief
                       {
                           PackageId = doc.PackageId,
                           Timestamp = doc.Timestamp,
                           UserId = doc.UserId,
                           SortIndex = doc.SortIndex
                       };

            this.Index(x => x.SortIndex, FieldIndexing.Default);
            this.Sort(x => x.SortIndex, SortOptions.Long);
        }
    }
}