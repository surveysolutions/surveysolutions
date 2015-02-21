using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IQueryableReadSideRepositoryReader<SynchronizationDeltaMetaInformation> queryableStorage;
        private readonly IReadSideKeyValueStorage<SynchronizationDeltaContent> contentStorage;
        private string queryByBriefFieldsIndexName = typeof(SynchronizationDeltasByBriefFields).Name;
        private string queryByRecordSizeIndexName = typeof(SynchronizationDeltasByRecordSize).Name;

        public ReadSideChunkReader(IQueryableReadSideRepositoryReader<SynchronizationDeltaMetaInformation> queryableStorage, IReadSideRepositoryIndexAccessor indexAccessor, IReadSideKeyValueStorage<SynchronizationDeltaContent> contentStorage)
        {
            this.queryableStorage = queryableStorage;
            this.indexAccessor = indexAccessor;
            this.contentStorage = contentStorage;
        }

        public SyncItem ReadChunk(string id)
        {
            var itemContent = contentStorage.GetById(id);

            if (itemContent == null)
                throw new ArgumentException("chunk is absent");

            return new SyncItem
                {
                    RootId = itemContent.RootId,
                    IsCompressed = itemContent.IsCompressed,
                    ItemType = itemContent.ItemType,
                    Content = itemContent.Content,
                    MetaInfo = itemContent.MetaInfo
                };
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(string lastSyncedPackageId, IEnumerable<Guid> users)
        {
            var userIds = users.Concat(new[] { Guid.Empty });

            if (lastSyncedPackageId == null)
            {
                List<SynchronizationDeltaMetaInformation> fullStreamDeltas = GetAllSynchronizationDeltaMetaInformation(
                    x => x.UserId.In(userIds))
                    .OrderBy(x => x.SortIndex).ToList();

                var fullListResult = fullStreamDeltas.Select(s => new SynchronizationChunkMeta(s.PublicKey))
                                                     .ToList();
                return fullListResult; 
            }

            SynchronizationDeltaMetaInformation lastSyncedPackage =
                this.indexAccessor.Query<SynchronizationDeltaMetaInformation>(queryByBriefFieldsIndexName)
                    .FirstOrDefault(x => x.PublicKey == lastSyncedPackageId);

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            var deltas = GetAllSynchronizationDeltaMetaInformation(x => x.SortIndex > lastSyncedPackage.SortIndex && x.UserId.In(userIds))
                              .OrderBy(x => x.SortIndex)
                              .ToList();

            var result = deltas.Select(s => new SynchronizationChunkMeta(s.PublicKey)).ToList();
            return result; 
        }

        private List<SynchronizationDeltaMetaInformation> GetAllSynchronizationDeltaMetaInformation(Expression<Func<SynchronizationDeltaMetaInformation, bool>> condition)
        {
            var result = new List<SynchronizationDeltaMetaInformation>();
            int skipResults = 0;
            while (true)
            {
                var chunk =
                    this.indexAccessor.Query<SynchronizationDeltaMetaInformation>(queryByBriefFieldsIndexName)
                        .Where(condition).Skip(skipResults).ToList();

                if (!chunk.Any())
                    break;
                result.AddRange(chunk);
                skipResults = result.Count;
            }
            return result;
        }

        public SynchronizationChunkMeta GetChunkMetaDataByTimestamp(DateTime timestamp, IEnumerable<Guid> users)
        {
            var items = this.indexAccessor.Query<SynchronizationDeltaMetaInformation>(queryByBriefFieldsIndexName);
            var userIds = users.Concat(new[] { Guid.Empty });

            SynchronizationDeltaMetaInformation meta = items.Where(x => timestamp >= x.Timestamp && x.UserId.In(userIds))
                                             .ToList()
                                             .OrderBy(x => x.SortIndex) 
                                             .Last();
            
            return new SynchronizationChunkMeta(meta.PublicKey);
        }

        public int GetNumberOfSyncPackagesWithBigSize()
        {
            var items = this.indexAccessor.Query<SynchronizationDeltaMetaInformation>(queryByRecordSizeIndexName);
            int count = items.Count();
            return count;
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
