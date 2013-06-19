using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Synchronization.SyncStorage
{
    public class ReadSideChunkStorage : IChunkStorage
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
                var sequences = queryableStorage.Query(_ => _.Select(d => d.Sequence));
                if (sequences.Any())
                    currentSequence = sequences.Max() + 1;
            }
            // catch here is in case on rebuild read layer
            // if execption was throwed this mean we have current Sequence equal to 1
            catch (MaintenanceException)
            {
              
            }
            
           
        }

        public void StoreChunk(Guid id, string syncItem, Guid userId)
        {
            lock (myLock)
            {
                storage.Store(new SynchronizationDelta(id, syncItem, CurrentSequence, userId), id);
                CurrentSequence++;
            /*    var oldItems =
                    queryableStorage.Query(
                        _ => _.Where(d => d.PublicKey == id).OrderByDescending(s => s.Sequence).Select(d => d.PublicKey));
                foreach (var oldItem in oldItems.Skip(1))
                {
                    storage.Remove(oldItem);
                }*/
            }
        }

        public string ReadChunk(Guid id)
        {
            var item = storage.GetById(id);
            if(item==null)
                throw new ArgumentException("chunk is absent");
            return item.Content;
        }

        public IEnumerable<Guid> GetChunksCreatedAfterForUsers(long sequence, IEnumerable<Guid> users)
        {
            return queryableStorage.Query(_ => _.Where(d => d.Sequence > sequence && d.UserId.In(users)).Select(d => d.PublicKey)).Distinct();
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
