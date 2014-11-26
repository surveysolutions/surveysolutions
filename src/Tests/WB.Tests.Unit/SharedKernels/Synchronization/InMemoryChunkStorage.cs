using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.Synchronization
{
    internal class InMemoryChunkStorage : IChunkWriter, IChunkReader, IReadSideRepositoryCleaner
    {
        private readonly IDictionary<Guid, SyncItem> container;

        public InMemoryChunkStorage(IDictionary<Guid, SyncItem> container)
        {
            this.container = container;
        }

        public InMemoryChunkStorage()
            : this(new Dictionary<Guid, SyncItem>())
        {
        }
        public void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp)
        {
            this.container[syncItem.Id] = syncItem;
        }

        public void RemoveChunk(Guid Id)
        {
            this.container.Remove(Id);
        }

        public SyncItem ReadChunk(Guid id)
        {
            return this.container[id];
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(Guid? lastSyncedPackageId, IEnumerable<Guid> users)
        {
            throw new NotImplementedException();
        }

        public SynchronizationChunkMeta GetChunkMetaDataByTimestamp(DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> GetChunksCreatedAfterForUsers(DateTime timestamp, IEnumerable<Guid> users)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void EnableCache()
        {
            throw new NotImplementedException();
        }

        public void DisableCache()
        {
            throw new NotImplementedException();
        }

        public string GetReadableStatus()
        {
            throw new NotImplementedException();
        }

        public Type ViewType { get; private set; }
    }
}
