using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;

namespace WB.Core.Synchronization.SyncStorage
{

    internal class ReadSideChunkReader : IChunkReader
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;
        private readonly IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage;
        private string queryIndexName = typeof(SynchronizationDeltasByBriefFields).Name;

        public ReadSideChunkReader(IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage, IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.queryableStorage = queryableStorage;
            this.indexAccessor = indexAccessor;
        }

        public SyncItem ReadChunk(string id)
        {
            SynchronizationDelta item = queryableStorage.GetById(id);
            if (item == null)
                throw new ArgumentException("chunk is absent");

            return new SyncItem
                {
                    RootId = item.RootId,
                    IsCompressed = item.IsCompressed,
                    ItemType = item.ItemType,
                    Content = item.Content,
                    MetaInfo = item.MetaInfo
                };
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(string lastSyncedPackageId, IEnumerable<Guid> users)
        {
            var items = this.indexAccessor.Query<SynchronizationDelta>(queryIndexName);

            var userIds = users.Concat(new[] { Guid.Empty });

            if (lastSyncedPackageId == null)
            {
                List<SynchronizationDelta> fullStreamDeltas = items.Where(x => x.UserId.In(userIds))
                                                                   .OrderBy(x => x.SortIndex)
                                                                   .ToList();

                var fullListResult = fullStreamDeltas.Select(s => new SynchronizationChunkMeta(s.PublicKey))
                                                     .ToList();
                return fullListResult; 
            }

            SynchronizationDelta lastSyncedPackage = items.FirstOrDefault(x => x.PublicKey == lastSyncedPackageId);

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            var deltas = items.Where(x => x.SortIndex > lastSyncedPackage.SortIndex && x.UserId.In(userIds))
                              .OrderBy(x => x.SortIndex)
                              .ToList();

            var result = deltas.Select(s => new SynchronizationChunkMeta(s.PublicKey)).ToList();
            return result; 
        }

        public SynchronizationChunkMeta GetChunkMetaDataByTimestamp(DateTime timestamp)
        {
            var items = this.indexAccessor.Query<SynchronizationDelta>(queryIndexName);

            SynchronizationDelta meta = items.Where(x => timestamp < x.Timestamp)
                                             .ToList()
                                             .OrderBy(x => x.SortIndex) 
                                             .First();
            
            return new SynchronizationChunkMeta(meta.PublicKey);
        }
    }

    [Serializable]
    public class SyncPackageNotFoundException : Exception
    {
        public SyncPackageNotFoundException() {}

        public SyncPackageNotFoundException(string message)
            : base(message) {}

        public SyncPackageNotFoundException(string message, Exception inner)
            : base(message, inner) {}

        protected SyncPackageNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}
