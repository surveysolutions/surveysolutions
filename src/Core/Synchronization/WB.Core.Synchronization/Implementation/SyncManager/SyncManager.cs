using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Raven.Client.Linq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.SyncManager
{
    internal class SyncManager : ISyncManager
    {
        private readonly IReadSideKeyValueStorage<ClientDeviceDocument> devices;
        private readonly IIncomingPackagesQueue incomingQueue;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;
        private readonly IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage;
        private string queryIndexName = typeof(SynchronizationDeltasByBriefFields).Name;

        public SyncManager(IReadSideKeyValueStorage<ClientDeviceDocument> devices, 
            IIncomingPackagesQueue incomingQueue,
            ILogger logger, 
            ICommandService commandService, 
            IQueryableReadSideRepositoryReader<UserDocument> userStorage,
            IReadSideRepositoryIndexAccessor indexAccessor, 
            IQueryableReadSideRepositoryReader<SynchronizationDelta> queryableStorage)
        {
            this.devices = devices;
            this.incomingQueue = incomingQueue;
            this.logger = logger;
            this.commandService = commandService;
            this.userStorage = userStorage;
            this.indexAccessor = indexAccessor;
            this.queryableStorage = queryableStorage;
        }

        public HandshakePackage ItitSync(ClientIdentifier clientIdentifier)
        {
            if (clientIdentifier.ClientInstanceKey == Guid.Empty)
                throw new ArgumentException("ClientInstanceKey is incorrect.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientDeviceKey))
                throw new ArgumentException("ClientDeviceKey is incorrect.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientVersionIdentifier))
                throw new ArgumentException("ClientVersionIdentifier is incorrect.");

            return this.CheckAndCreateNewSyncActivity(clientIdentifier);
        }

        public void SendSyncItem(string item)
        {
            this.incomingQueue.PushSyncItem(item);
        }

        public void LinkUserToDevice(Guid interviewerId, string deviceId)
        {
            if (interviewerId == Guid.Empty)
                throw new ArgumentException("Interview id is not set.");

            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentException("Device id is not set.");

            commandService.Execute(new LinkUserToDevice(interviewerId, deviceId));
        }


        public IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, string lastSyncedPackageId)
        {
            var device = devices.GetById(clientRegistrationKey);

            if (device == null)
                throw new ArgumentException("Device was not found.");

            var users = new List<Guid> { userId };
            var items = this.indexAccessor.Query<SynchronizationDelta>(this.queryIndexName);

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

        public SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, string id)
        {
            SyncItem item = this.GetSyncItem(clientRegistrationId, id);

            if (item == null)
            {
                throw new Exception("Item was not found");
            }

            return new SyncPackage {Id = Guid.NewGuid(), SyncItem = item};
        }

        public string GetPackageIdByTimestamp(Guid userId, DateTime timestamp)
        {
            var users = this.GetUserTeamates(userId);
            var items = this.indexAccessor.Query<SynchronizationDelta>(this.queryIndexName);
            var userIds = users.Concat(new[] { Guid.Empty });

            SynchronizationDelta meta = items.Where(x => timestamp >= x.Timestamp && x.UserId.In(userIds))
                .ToList()
                .OrderBy(x => x.SortIndex)
                .Last();
            return new SynchronizationChunkMeta(meta.PublicKey).Id;
        }

        private IEnumerable<Guid> GetUserTeamates(Guid userId)
        {
            var user = userStorage.Query(_ => _.Where(u => u.PublicKey == userId)).ToList().FirstOrDefault();
            if (user == null)
                return Enumerable.Empty<Guid>();

            Guid supervisorId = user.Roles.Contains(UserRoles.Supervisor) ? userId : user.Supervisor.Id;

            var team =
                userStorage.Query(
                    _ => _.Where(u => u.Supervisor != null && u.Supervisor.Id == supervisorId).Select(u => u.PublicKey)).ToList();
            team.Add(supervisorId);
            return team;
        }

        private SyncItem GetSyncItem(Guid clientRegistrationKey, string id)
        {
            var device = devices.GetById(clientRegistrationKey);
            if (device == null)
                throw new ArgumentException("Device was not found.");

            // item could be changed on server and sequence would be different
            SynchronizationDelta delta = this.queryableStorage.GetById(id);
            if (delta == null)
                throw new ArgumentException("chunk is absent");
            var result = new SyncItem
                         {
                             RootId = delta.RootId,
                             IsCompressed = delta.IsCompressed,
                             ItemType = delta.ItemType,
                             Content = delta.Content,
                             MetaInfo = delta.MetaInfo
                         };
            return result;
        }

        private HandshakePackage CheckAndCreateNewSyncActivity(ClientIdentifier identifier)
        {
            Guid ClientRegistrationKey;

            ClientDeviceDocument device = null;
            if (identifier.ClientRegistrationKey.HasValue)
            {
                if (identifier.ClientRegistrationKey.Value == Guid.Empty)
                    throw new ArgumentException("Unknown device.");

                device = devices.GetById(identifier.ClientRegistrationKey.Value);
                if (device == null)
                {
                    throw new Exception("Device was not found. It could be linked to other system.");
                }

                //old sync devices are already in use
                //lets allow to sync them for awhile
                //then we have to delete this code
                if (device.SupervisorKey == Guid.Empty)
                {
                    logger.Error("Old registered device is synchronizing. Errors could be on client in case of different team.");
                }
                else if (device.SupervisorKey != identifier.SupervisorPublicKey)
                {
                    throw new Exception("Device was assigned to another Supervisor.");
                }

                //TODO: check device validity

                ClientRegistrationKey = device.PublicKey;
            }
            else //register new device
            {
                ClientRegistrationKey = Guid.NewGuid();

                commandService.Execute(new CreateClientDeviceCommand(ClientRegistrationKey,
                    identifier.ClientDeviceKey, identifier.ClientInstanceKey, identifier.SupervisorPublicKey));
            }

            Guid syncActivityKey = Guid.NewGuid();

            return new HandshakePackage(identifier.ClientInstanceKey, syncActivityKey, ClientRegistrationKey);
        }
    }
}