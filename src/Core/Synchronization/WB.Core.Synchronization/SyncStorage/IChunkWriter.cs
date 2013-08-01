using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal interface IChunkWriter
    {
        void StoreChunk(SyncItem syncItem, Guid? userId);
      
    }
}