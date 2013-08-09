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
                .ToList()
                .Select(s => new KeyValuePair<long, Guid>(s.Sequence, s.PublicKey));

            return elements.GroupBy(g => g.Value)
                   .Select(pair => pair.First(x => x.Key == pair.Max(y => y.Key)))
                   .OrderBy(o=>o.Key)
                   .ToList();
        }
    }
}
