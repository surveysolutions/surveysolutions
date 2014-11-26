using System;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkWriter : IChunkWriter
    {
        private readonly IQueryableReadSideRepositoryWriter<SynchronizationDelta> storage;
        private readonly IArchiveUtils archiver;
        private bool cacheEnabled = false;
        private int currentSortIndex = 0;
        public ReadSideChunkWriter(IQueryableReadSideRepositoryWriter<SynchronizationDelta> storage, IArchiveUtils archiver)
        {
            this.storage = storage;
            this.archiver = archiver;
        }

        public void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp)
        {
            var content = syncItem.IsCompressed ? archiver.CompressString(syncItem.Content) : syncItem.Content;
            var metaInfo = (syncItem.IsCompressed && !string.IsNullOrWhiteSpace(syncItem.MetaInfo)) ? archiver.CompressString(syncItem.MetaInfo) : syncItem.MetaInfo;

            int sortIndex = 0;
            if (cacheEnabled)
            {
                sortIndex = currentSortIndex;
                currentSortIndex++;
            }
            else
            {
                var query = storage.Query(_ => _.OrderByDescending(x => x.SortIndex).Select(x => x.SortIndex));
                if (query.Any())
                    sortIndex = query.First();
            }

            var synchronizationDelta = new SynchronizationDelta(syncItem.Id, content, timestamp, userId, syncItem.IsCompressed, syncItem.ItemType, metaInfo, sortIndex);

            storage.Store(synchronizationDelta, syncItem.Id);
        }

        public void Clear()
        {
            var readSideRepositoryCleaner = storage as IReadSideRepositoryCleaner;
            if (readSideRepositoryCleaner != null)
                readSideRepositoryCleaner.Clear();
        }

        public void EnableCache()
        {
            var readSideRepositoryWriter = storage as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.EnableCache();

            cacheEnabled = true;
            currentSortIndex = 0;
        }

        public void DisableCache()
        {
            var readSideRepositoryWriter = storage as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.DisableCache();

            cacheEnabled = false;
        }

        public string GetReadableStatus()
        {
            var readSideRepositoryWriter = storage as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                return readSideRepositoryWriter.GetReadableStatus();
            return "";
        }

        public Type ViewType { get { return typeof (SynchronizationDelta); } }
    }
}
