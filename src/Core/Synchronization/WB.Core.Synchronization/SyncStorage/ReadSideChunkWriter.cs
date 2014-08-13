using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkWriter : IChunkWriter
    {
        private IQueryableReadSideRepositoryWriter<SynchronizationDelta> storage;

        public ReadSideChunkWriter(IQueryableReadSideRepositoryWriter<SynchronizationDelta> storage)
        {
            this.storage = storage;
        }

        public void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp)
        {
            storage.Store(new SynchronizationDelta(syncItem.Id, syncItem.Content, timestamp, userId, syncItem.IsCompressed,
                syncItem.ItemType, syncItem.MetaInfo), syncItem.Id);

        }
    }
}
