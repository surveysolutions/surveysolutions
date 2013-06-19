using System;
using System.Collections.Generic;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkStorage
    {
        void StoreChunk(Guid id, string syncItem, Guid userId);
        string ReadChunk(Guid id);
        IEnumerable<Guid> GetChunksCreatedAfterForUsers(long sequence, IEnumerable<Guid> users);
    }
}