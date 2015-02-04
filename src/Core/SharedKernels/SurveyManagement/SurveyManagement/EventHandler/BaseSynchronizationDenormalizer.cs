using System;
using System.Linq;
using System.Threading;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal abstract class BaseSynchronizationDenormalizer : BaseDenormalizer
    {
        protected readonly bool useCompression;
        private readonly IArchiveUtils archiver;
        private readonly IJsonUtils jsonUtils;
        protected readonly IReadSideRepositoryWriter<SynchronizationDelta> syncStorage;
        protected readonly IQueryableReadSideRepositoryReader<SynchronizationDelta> syncStorageReader;
        private static int currentSortIndex = 0;

        protected BaseSynchronizationDenormalizer(IArchiveUtils archiver,
            IJsonUtils jsonUtils, 
            IReadSideRepositoryWriter<SynchronizationDelta> syncStorage, 
            IQueryableReadSideRepositoryReader<SynchronizationDelta> syncStorageReader)
        {
            this.archiver = archiver;
            this.jsonUtils = jsonUtils;
            this.syncStorage = syncStorage;
            this.syncStorageReader = syncStorageReader;
            this.useCompression = true;
        }

        public void StoreChunk(SyncItem syncItem, Guid? userId, DateTime timestamp)
        {
            int sortIndex = 0;

            var syncWriter = this.syncStorage as IReadSideRepositoryWriter;

            if (syncWriter != null && syncWriter.IsCacheEnabled)
            {
                sortIndex = currentSortIndex;
                Interlocked.Increment(ref currentSortIndex);
            }
            else
            {
                var query = this.syncStorageReader.Query(_ => _.OrderByDescending(x => x.SortIndex).Select(x => x.SortIndex));
                if (query.Any())
                    sortIndex = query.First() + 1;
            }

            var synchronizationDelta = new SynchronizationDelta(syncItem.RootId, syncItem.Content, timestamp,
                userId, syncItem.IsCompressed, syncItem.ItemType, syncItem.MetaInfo, sortIndex);

            syncStorage.Store(synchronizationDelta, synchronizationDelta.PublicKey);
        }

        protected SyncItem CreateSyncItem(Guid id, string itemType, string rawContent, string rawMeta)
        {
            var content = this.useCompression ? this.archiver.CompressString(rawContent) : rawContent;
            var metaInfo = (this.useCompression && !string.IsNullOrWhiteSpace(rawMeta)) ? this.archiver.CompressString(rawMeta) : rawMeta;

            return new SyncItem
                   {
                       RootId = id,
                       ItemType = itemType,
                       IsCompressed = this.useCompression,
                       Content = content,
                       MetaInfo = metaInfo
                   };
        }

        protected string GetItemAsContent(object item)
        {
            return this.jsonUtils.Serialize(item, TypeSerializationSettings.AllTypes) ;
        }
    }
}