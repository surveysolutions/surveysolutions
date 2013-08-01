using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkStorage : IChunkStorage
    {
        private IReadSideRepositoryWriter<SynchronizationDelta> storage;
        private IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage;
        private readonly object myLock = new object();
        private long? currentSequence;

        public ReadSideChunkStorage(IReadSideRepositoryWriter<SynchronizationDelta> storage,
                                    IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage)
        {
            this.storage = storage;
            this.queryableStorage = queryableStorage;
        }

        private void DefineCurrentSequence()
        {
            currentSequence = 1;

            try
            {
                var sequences = queryableStorage.QueryAll(q => true).Select(d => d.Sequence)/*Query(_ => _.Select(d => d.Sequence))*/.ToList();
                if (sequences.Any())
                    currentSequence = sequences.Max() + 1;
            }
            // catch here is in case on rebuild read layer
            // if execption was throwed this mean we have current Sequence equal to 1
            catch (MaintenanceException)
            {
            }
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

        public SyncItem ReadChunk(Guid id)
        {
            var item = storage.GetById(id);
            if (item == null)
                throw new ArgumentException("chunk is absent");

            return new SyncItem()
                {
                    Id = item.PublicKey,
                    IsCompressed = item.IsCompressed,
                    ItemType = item.ItemType,
                    Content = item.Content,
                    MetaInfo = item.MetaInfo
                };
        }

        public IEnumerable<Guid> GetChunksCreatedAfterForUsers(long sequence, IEnumerable<Guid> users)
        {
            return queryableStorage.QueryAll(d => d.Sequence > sequence && (d.UserId.HasValue && d.UserId.Value.In(users) || !d.UserId.HasValue))
                .Select(d => d.PublicKey)
                .Distinct()
                .ToList();
        }

        public IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(long sequence, IEnumerable<Guid> users)
        {
            //todo: query is not optimal but will be replaced shortly
            var elements = queryableStorage.QueryAll(d => d.Sequence > sequence && (d.UserId.HasValue && d.UserId.Value.In(users) || !d.UserId.HasValue))
                .Select(d => d)
                .ToList()
                .Select(s => new KeyValuePair<long, Guid>(s.Sequence, s.PublicKey))
                ;

            return elements.GroupBy(g => g.Value)
                   .Select(pair => pair.First(x => x.Key == pair.Max(y => y.Key)))
                   .OrderBy(o=>o.Key)
                   .ToList();
        }

        public IEnumerable<SyncItem> GetChunks(long sequence, IEnumerable<Guid> users)
        {
            return queryableStorage.QueryAll(d => d.Sequence > sequence && (d.UserId.HasValue && d.UserId.Value.In(users) || !d.UserId.HasValue))
                .Select(d => new SyncItem())
                .Distinct()
                .ToList();
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
