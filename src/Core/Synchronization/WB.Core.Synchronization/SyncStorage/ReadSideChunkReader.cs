using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkReader : IChunkReader
    {
        private readonly IQueryableReadSideRepositoryWriter<SynchronizationDelta> queryableStorage;

        public ReadSideChunkReader(IQueryableReadSideRepositoryWriter<SynchronizationDelta> queryableStorage)
        {
            this.queryableStorage = queryableStorage;
        }


        public SyncItem ReadChunk(Guid id)
        {
            var item = queryableStorage.GetById(id);
            if (item == null)
                throw new ArgumentException("chunk is absent");

            return new SyncItem
                {
                    Id = item.PublicKey,
                    IsCompressed = item.IsCompressed,
                    ItemType = item.ItemType,
                    Content = item.Content,
                    MetaInfo = item.MetaInfo
                };
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(Guid? lastSyncedPackageId, IEnumerable<Guid> users)
        {
            var userIds = users.Concat(new[] { Guid.Empty });

            if (lastSyncedPackageId == null)
            {
                var fullStreamDeltas = queryableStorage.Query(_ => _.Where(x => x.UserId.In(userIds))
                                                                    .OrderBy(x => x.SortIndex)
                                                                    .ToList());

                var fullListResult = fullStreamDeltas.Select(s => new SynchronizationChunkMeta(s.PublicKey))
                                                     .ToList();
                return fullListResult; 
            }

            SynchronizationDelta lastSyncedPackage = queryableStorage.Query(_ => _.FirstOrDefault(x => x.PublicKey == lastSyncedPackageId));

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            var deltas = queryableStorage.Query(_ => _.Where(x => x.SortIndex > lastSyncedPackage.SortIndex && x.UserId.In(userIds))
                                                      .OrderBy(x => x.SortIndex)
                                                      .ToList());

            var result = deltas.Select(s => new SynchronizationChunkMeta(s.PublicKey))
                               .ToList();
            return result; 
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
