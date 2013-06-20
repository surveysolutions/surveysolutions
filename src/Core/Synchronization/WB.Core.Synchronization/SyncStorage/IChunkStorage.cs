using System;
using System.Collections.Generic;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkStorage
    {
        void StoreChunk(Guid id, string syncItem);
        string ReadChunk(Guid id);
        IEnumerable<Guid> GetChunksCreatedAfter(long sequence);

        IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(long sequence);
    }
}