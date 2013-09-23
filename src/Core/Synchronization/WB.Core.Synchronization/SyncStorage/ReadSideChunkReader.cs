using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class ReadSideChunkReader : IChunkReader
    {
        private readonly IQueryableReadSideRepositoryWriter<SynchronizationDelta> queryableStorage;

        public ReadSideChunkReader(IQueryableReadSideRepositoryWriter<SynchronizationDelta> queryableStorage)
        {
            this.queryableStorage = queryableStorage;
        }


        public SyncItem ReadChunk(Guid id)
        {
            var item = queryableStorage.GetById(id);
            if (item == null)
                throw new ArgumentException("chunk is absent");

            return new SyncItem
                {
                    Id = item.PublicKey,
                    IsCompressed = item.IsCompressed,
                    ItemType = item.ItemType,
                    Content = item.Content,
                    MetaInfo = item.MetaInfo
                };
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(long sequence, IEnumerable<Guid> users)
        {
            //todo: query is not optimal but will be replaced shortly
            return
                queryableStorage.QueryAll(
                    d => d.Sequence > sequence && (d.UserId.HasValue && d.UserId.Value.In(users) || !d.UserId.HasValue))
                                .OrderBy(o => o.Sequence).Where((package) => FilterDeleteNotificationsWithInterviewWasnotSend(package,sequence))
                                .Select(s => new SynchronizationChunkMeta(s.PublicKey, s.Sequence)).ToList();
        }

        private bool FilterDeleteNotificationsWithInterviewWasnotSend(SynchronizationDelta package, long sequence)
        {
            if (package.ItemType != SyncItemType.DeleteQuestionnare)
                return true;
            if (sequence > 0)
                return true;
            return false;
        }
    }
}
