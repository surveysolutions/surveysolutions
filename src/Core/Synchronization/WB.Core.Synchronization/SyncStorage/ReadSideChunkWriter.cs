using System;
using System.Linq;
using System.Threading;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkWriter : IChunkWriter
    {
        private const string CounterId = "counter";
        private readonly IReadSideRepositoryWriter<SynchronizationDeltaMetaInformation> storage;
        private readonly IReadSideKeyValueStorage<SynchronizationDeltaContent> contentStorage;
        private readonly IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage;
        private static readonly object StoreSyncDeltaLockObject = new object();

        public ReadSideChunkWriter(
            IReadSideRepositoryWriter<SynchronizationDeltaMetaInformation> storage,
            IReadSideKeyValueStorage<SynchronizationDeltaContent> contentStorage,
            IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage)
        {
            this.storage = storage;
            this.contentStorage = contentStorage;
            this.counterStorage = counterStorage;
        }

        public void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp)
        {
            lock (StoreSyncDeltaLockObject)
            {
                SynchronizationDeltasCounter deltasCounter = this.counterStorage.GetById(CounterId);
                int storedDeltasCount = deltasCounter != null ? deltasCounter.CountOfStoredDeltas : 0;

                int nextSortIndex = storedDeltasCount;

                storedDeltasCount++;
                this.counterStorage.Store(new SynchronizationDeltasCounter(storedDeltasCount), CounterId);

                var synchronizationDelta = new SynchronizationDeltaMetaInformation(syncItem.RootId, timestamp,
                    userId, syncItem.ItemType, nextSortIndex,
                    string.IsNullOrEmpty(syncItem.Content) ? 0 : syncItem.Content.Length,
                    string.IsNullOrEmpty(syncItem.MetaInfo) ? 0 : syncItem.MetaInfo.Length);

                this.storage.Store(synchronizationDelta, synchronizationDelta.PublicKey);

                this.contentStorage.Store(
                    new SynchronizationDeltaContent(synchronizationDelta.PublicKey, syncItem.Content, syncItem.MetaInfo,
                        syncItem.IsCompressed, syncItem.ItemType, syncItem.RootId), synchronizationDelta.PublicKey);
            }
        }

        public void Clear()
        {
            var readSideRepositoryCleaner = storage as IReadSideRepositoryCleaner;
            if (readSideRepositoryCleaner != null)
                readSideRepositoryCleaner.Clear();
        }

        public void EnableCache()
        {
            var readSideRepositoryWriter = storage as IChacheableRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.EnableCache();
        }

        public void DisableCache()
        {
            var readSideRepositoryWriter = storage as IChacheableRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.DisableCache();
        }

        public string GetReadableStatus()
        {
            var readSideRepositoryWriter = storage as IChacheableRepositoryWriter;
            if (readSideRepositoryWriter != null)
                return readSideRepositoryWriter.GetReadableStatus();
            return "";
        }

        public Type ViewType { get { return typeof (SynchronizationDeltaMetaInformation); } }
    }
}
