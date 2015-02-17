using System;
using System.Linq;
using Raven.Client.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.ReadSide.Indexes
{
    public class SynchronizationDeltasByRecordSize :
        AbstractIndexCreationTask<SynchronizationDeltaMetaInformation, SynchronizationDeltasByRecordSize.Result>
    {
        private const int WarningLength = 2097152;

        public class Result
        {
            public Guid RootId { get; set; }
            public int ContentSize { get; set; }
            public int MetaInfoSize { get; set; }
            public int Count { get; set; }
        }

        public SynchronizationDeltasByRecordSize()
        {
            Map = deltas => from delta in deltas
                            where delta.ContentLength + delta.MetaDataLength > WarningLength
                            select new
                            {
                                RootId = delta.RootId,
                                ContentSize = delta.ContentLength,
                                MetaInfoSize = delta.MetaDataLength,
                                Count = 1
                            };

            Reduce = results => from result in results
                                group result by result.RootId
                                into g
                                select new
                                    {
                                        RootId = g.Key,
                                        ContentSize = g.Max(x => x.ContentSize),
                                        MetaInfoSize = g.Max(x => x.MetaInfoSize),
                                        Count = g.Sum(x => x.Count)
                                    };

        }
    }
}