using System;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkStorage
    {
        void StoreChunk(Guid id, string syncItem);
        string ReadChunk(Guid id);
    }
}