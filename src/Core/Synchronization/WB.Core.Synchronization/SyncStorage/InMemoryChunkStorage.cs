using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class InMemoryChunkStorage : IChunkWriter, IChunkReader
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
            return container[id];
        }

        public IEnumerable<Guid> GetChunksCreatedAfterForUsers(DateTime timestamp, IEnumerable<Guid> users)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(DateTime timestamp, IEnumerable<Guid> users)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(DateTime timestamp)
        {
            throw new NotImplementedException();
        }
    }
}
