using System;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkWriter : IChunkWriter
    {
        private readonly IQueryableReadSideRepositoryWriter<SynchronizationDelta> storage;
        private readonly IArchiveUtils archiver;

        public ReadSideChunkWriter(IQueryableReadSideRepositoryWriter<SynchronizationDelta> storage, IArchiveUtils archiver)
        {
            this.storage = storage;
            this.archiver = archiver;
        }

        public void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp)
        {
            var content = syncItem.IsCompressed ? archiver.CompressString(syncItem.Content) : syncItem.Content;
            var metaInfo = (syncItem.IsCompressed && !string.IsNullOrWhiteSpace(syncItem.MetaInfo)) ? archiver.CompressString(syncItem.MetaInfo) : syncItem.MetaInfo; 

            storage.Store(new SynchronizationDelta(syncItem.Id, content, timestamp, userId, syncItem.IsCompressed,
                syncItem.ItemType, metaInfo), syncItem.Id);

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
        }

        public void DisableCache()
        {
            var readSideRepositoryWriter = storage as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.DisableCache();
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
