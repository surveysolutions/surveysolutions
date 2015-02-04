using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class SimpleSynchronizationDataStorage : ISynchronizationDataStorage
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;
        private readonly IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage;
        private string queryIndexName = typeof(SynchronizationDeltasByBriefFields).Name;

        public SimpleSynchronizationDataStorage(
            IQueryableReadSideRepositoryReader<UserDocument> userStorage,
            IReadSideRepositoryIndexAccessor indexAccessor, 
            IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage)
        {
            this.userStorage = userStorage;
            this.indexAccessor = indexAccessor;
            this.queryableStorage = queryableStorage;
        }
     
        public SyncItem GetLatestVersion(string id)
        {
            var result = ReadChunk(id);
            return result;
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(string lastSyncedPackageId, Guid userId)
        {
            var users = new List<Guid> { userId };
            return GetChunkMetaDataCreatedAfter(lastSyncedPackageId, users);
        }

        private IEnumerable<Guid> GetUserTeamates(Guid userId)
        {
            var user = userStorage.Query(_ => _.Where(u => u.PublicKey == userId)).ToList().FirstOrDefault();
            if (user == null)
                return Enumerable.Empty<Guid>();

            Guid supervisorId = user.Roles.Contains(UserRoles.Supervisor) ? userId : user.Supervisor.Id;

            var team=
                userStorage.Query(
                    _ => _.Where(u => u.Supervisor != null && u.Supervisor.Id == supervisorId).Select(u => u.PublicKey)).ToList();
            team.Add(supervisorId);
            return team;
        }

        public SynchronizationChunkMeta GetChunkInfoByTimestamp(DateTime timestamp, Guid userId)
        {
            var users = GetUserTeamates(userId);
            return this.GetChunkMetaDataByTimestamp(timestamp, users);
        }

        public SyncItem ReadChunk(string id)
        {
            SynchronizationDelta item = queryableStorage.GetById(id);
            if (item == null)
                throw new ArgumentException("chunk is absent");

            return new SyncItem
            {
                RootId = item.RootId,
                IsCompressed = item.IsCompressed,
                ItemType = item.ItemType,
                Content = item.Content,
                MetaInfo = item.MetaInfo
            };
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(string lastSyncedPackageId, IEnumerable<Guid> users)
        {
            var items = this.indexAccessor.Query<SynchronizationDelta>(queryIndexName);

            var userIds = users.Concat(new[] { Guid.Empty });

            if (lastSyncedPackageId == null)
            {
                List<SynchronizationDelta> fullStreamDeltas = items.Where(x => x.UserId.In(userIds))
                                                                   .OrderBy(x => x.SortIndex)
                                                                   .ToList();

                var fullListResult = fullStreamDeltas.Select(s => new SynchronizationChunkMeta(s.PublicKey))
                                                     .ToList();
                return fullListResult;
            }

            SynchronizationDelta lastSyncedPackage = items.FirstOrDefault(x => x.PublicKey == lastSyncedPackageId);

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            var deltas = items.Where(x => x.SortIndex > lastSyncedPackage.SortIndex && x.UserId.In(userIds))
                              .OrderBy(x => x.SortIndex)
                              .ToList();

            var result = deltas.Select(s => new SynchronizationChunkMeta(s.PublicKey)).ToList();
            return result;
        }

        public SynchronizationChunkMeta GetChunkMetaDataByTimestamp(DateTime timestamp, IEnumerable<Guid> users)
        {
            var items = this.indexAccessor.Query<SynchronizationDelta>(queryIndexName);
            var userIds = users.Concat(new[] { Guid.Empty });

            SynchronizationDelta meta = items.Where(x => timestamp >= x.Timestamp && x.UserId.In(userIds))
                                             .ToList()
                                             .OrderBy(x => x.SortIndex)
                                             .Last();

            return new SynchronizationChunkMeta(meta.PublicKey);
        }
    }
}
