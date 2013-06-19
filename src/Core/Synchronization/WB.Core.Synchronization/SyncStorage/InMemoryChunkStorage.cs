using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InMemoryChunkStorage : IChunkStorage
    {
        private readonly IDictionary<Guid, SynchronizationDelta> container;

        public InMemoryChunkStorage(IDictionary<Guid, SynchronizationDelta> container)
        {
            this.container = container;
        }

        public InMemoryChunkStorage()
            : this(new Dictionary<Guid, SynchronizationDelta>())
        {
        }
        public void StoreChunk(Guid id, string syncItem, Guid userId)
        {
            this.container[id] = new SynchronizationDelta(id, syncItem, 0, userId);
        }

        public string ReadChunk(Guid id)
        {
            return container[id].Content;
        }

        public IEnumerable<Guid> GetChunksCreatedAfterForUsers(long sequence, IEnumerable<Guid> users)
        {
            throw new NotImplementedException();
        }

    }
}
