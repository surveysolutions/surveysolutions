using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal interface IChunkWriter : IReadSideRepositoryCleaner, IReadSideRepositoryWriter
    {
        void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp);
    }
}