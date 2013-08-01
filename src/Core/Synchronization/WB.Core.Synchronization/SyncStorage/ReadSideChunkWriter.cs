using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkWriter : IChunkWriter
    {
        private IQuerableReadSideRepositoryWriter<SynchronizationDelta> storage;
        private readonly object myLock = new object();
        private long? currentSequence;

        public ReadSideChunkWriter(IQuerableReadSideRepositoryWriter<SynchronizationDelta> storage)
        {
            this.storage = storage;
        }

        private void DefineCurrentSequence()
        {
            currentSequence = 1;

            var sequences =
                storage.Query(
                    _ => _.OrderByDescending(d => d.Sequence).Select(d => d.Sequence).Take(1).ToList());
            if (sequences.Any())
                currentSequence = sequences.First() + 1;

        }

        public void StoreChunk(SyncItem syncItem, Guid? userId)
        {
            lock (myLock)
            {
                storage.Store(new SynchronizationDelta(syncItem.Id, syncItem.Content, CurrentSequence, userId, syncItem.IsCompressed,
                                                    syncItem.ItemType, syncItem.MetaInfo), syncItem.Id);
                CurrentSequence++;
            }
        }

        public void RemoveChunk(Guid Id)
        {
            lock (myLock)
            {
                storage.Remove(Id);
            }
        }


        protected long CurrentSequence
        {
            get
            {
                if (!currentSequence.HasValue)
                    DefineCurrentSequence();
                return currentSequence.Value;
            }
            set { currentSequence = value; }
        }
    }
}
