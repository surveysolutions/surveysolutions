using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkStorage
    {
        void StoreChunk(SyncItem syncItem, Guid? userId);
        SyncItem ReadChunk(Guid id);
        IEnumerable<Guid> GetChunksCreatedAfterForUsers(long sequence, IEnumerable<Guid> users);
    }
}