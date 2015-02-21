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
        private readonly IReadSideRepositoryWriter<SynchronizationDeltaMetaInformation> storage;
        private readonly IReadSideKeyValueStorage<SynchronizationDeltaContent> contentStorage;
        private readonly IQueryableReadSideRepositoryReader<SynchronizationDeltaMetaInformation> storageReader;
        private bool cacheEnabled = false;
        private static int currentSortIndex = 0;

        public ReadSideChunkWriter(IReadSideRepositoryWriter<SynchronizationDeltaMetaInformation> storage,
            IQueryableReadSideRepositoryReader<SynchronizationDeltaMetaInformation> storageReader,
            IReadSideKeyValueStorage<SynchronizationDeltaContent> contentStorage)
        {
            this.storage = storage;
            this.storageReader = storageReader;
            this.contentStorage = contentStorage;
        }

        public void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp)
        {
            int sortIndex = 0;
            if (cacheEnabled)
            {
                sortIndex = currentSortIndex;
                Interlocked.Increment(ref currentSortIndex);
            }
            else
            {
                var query = storageReader.Query(_ => _.OrderByDescending(x => x.SortIndex).Select(x => x.SortIndex));
                if (query.Any())
                    sortIndex = query.First() + 1;
            }

            var synchronizationDelta = new SynchronizationDeltaMetaInformation(syncItem.RootId, timestamp,
                userId, syncItem.ItemType, sortIndex, string.IsNullOrEmpty(syncItem.Content) ? 0 : syncItem.Content.Length,
                string.IsNullOrEmpty(syncItem.MetaInfo) ? 0 : syncItem.MetaInfo.Length);

            storage.Store(synchronizationDelta, synchronizationDelta.PublicKey);

            contentStorage.Store(
                new SynchronizationDeltaContent(synchronizationDelta.PublicKey, syncItem.Content, syncItem.MetaInfo,
                    syncItem.IsCompressed, syncItem.ItemType, syncItem.RootId), synchronizationDelta.PublicKey);
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

            cacheEnabled = true;
            currentSortIndex = 0;
        }

        public void DisableCache()
        {
            var readSideRepositoryWriter = storage as IChacheableRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.DisableCache();

            cacheEnabled = false;
            currentSortIndex = 0;
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
