using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    public class SynchronizationDeltasByRecordSize : AbstractMultiMapIndexCreationTask<SynchronizationDeltasByRecordSize.Result>
    {
        private const int WarningLength = 2097152;

        public class Result
        {
            public int GroupingKey { get; set; }
            public int Count { get; set; }
        }

        public SynchronizationDeltasByRecordSize()
        {
            AddMap<InterviewSyncPackageMeta>(packages => from package in packages
                    where package.ContentSize + package.MetaInfoSize > WarningLength
                    select new Result
                        {
                            GroupingKey = 1,
                            Count = 1
                        });

            AddMap<QuestionnaireSyncPackageMeta>(packages => from package in packages
                    where package.ContentSize + package.MetaInfoSize > WarningLength
                    select new Result
                        {
                            GroupingKey = 1,
                            Count = 1
                        });

            Reduce = results => from result in results
                                group result by result.GroupingKey into g
                                select new
                                {
                                    GroupingKey = g.Key,
                                    Count = g.Sum(x => x.Count)
                                };

            Index(x => x.GroupingKey, FieldIndexing.Analyzed);
        }
    }
}
