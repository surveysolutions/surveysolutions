using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InMemoryChunkStorage : IChunkStorage
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
        public void StoreChunk(SyncItem syncItem, Guid? userId)
        {
            this.container[syncItem.Id] = syncItem;
        }

        public SyncItem ReadChunk(Guid id)
        {
            return container[id];
        }

        public IEnumerable<Guid> GetChunksCreatedAfterForUsers(long sequence, IEnumerable<Guid> users)
        {
            throw new NotImplementedException();
        }

    }
}
