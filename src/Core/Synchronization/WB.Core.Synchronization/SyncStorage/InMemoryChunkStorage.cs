using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InMemoryChunkStorage : IChunkStorage
    {
        private readonly IDictionary<Guid, string> container;

        public InMemoryChunkStorage(IDictionary<Guid, string> container)
        {
            this.container = container;
        }

        public InMemoryChunkStorage()
            : this(new Dictionary<Guid, string>())
        {
        }

        public void StoreChunk(Guid id, string syncItem)
        {
            container.Add(id, syncItem);
        }

        public string ReadChunk(Guid id)
        {
            return container[id];
        }

        public IEnumerable<Guid> GetChunksCreatedAfter(long sequence)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(long sequence)
        {
            throw new NotImplementedException();
        }
    }
}
