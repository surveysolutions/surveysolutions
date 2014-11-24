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

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(DateTime timestamp, IEnumerable<Guid> users)
        {
            var userIds = users.Concat(new[] { Guid.Empty });

            return
                queryableStorage.QueryAll(
                    d => d.Timestamp > timestamp && d.UserId.In(userIds))
                                .OrderBy(o => o.Timestamp)
                                .Select(s => new SynchronizationChunkMeta(s.PublicKey, s.Timestamp.Ticks)).ToList();
        }
    }
}
