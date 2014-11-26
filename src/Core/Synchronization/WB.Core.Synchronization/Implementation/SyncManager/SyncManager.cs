using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.Implementation.SyncManager
{
    internal class SyncManager : ISyncManager
    {
        private readonly IQueryableReadSideRepositoryWriter<ClientDeviceDocument> devices;
        private readonly ISynchronizationDataStorage storage;
        private readonly IIncomePackagesRepository incomeRepository;
        private readonly ILogger logger;
        private readonly ICommandService commandService;

        public SyncManager(IQueryableReadSideRepositoryWriter<ClientDeviceDocument> devices,
            ISynchronizationDataStorage storage, 
            IIncomePackagesRepository incomeRepository,
            ILogger logger, 
            ICommandService commandService)
        {
            this.devices = devices;
            this.storage = storage;
            this.incomeRepository = incomeRepository;
            this.logger = logger;
            this.commandService = commandService;
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

        public bool SendSyncItem(SyncItem item)
        {
            if (item == null)
                throw new ArgumentException("Sync Item is not set.");

            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item content is not set.");

            if (item.Id == Guid.Empty)
                throw new ArgumentException("Sync Item id is not set.");

            this.incomeRepository.StoreIncomingItem(item);
            return true;
        }

        public IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, Guid? lastSyncedPackageId)
        {
            var device = devices.GetById(clientRegistrationKey);

            if (device == null)
                throw new ArgumentException("Device was not found.");

            return storage.GetChunkPairsCreatedAfter(lastSyncedPackageId, userId);
        }

        public SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, Guid id)
        {
            var syncPackage = new SyncPackage();

            SyncItem item = this.GetSyncItem(clientRegistrationId, id);

            if (item == null)
            {
                throw new Exception("Item was not found");
            }

            syncPackage.ItemsContainer.Add(item);
            
            return syncPackage;
        }

        public Guid GetPackageIdByTimestamp(DateTime timestamp)
        {
            return this.storage.GetChunkInfoByTimestamp(timestamp).Id;
        }

        private SyncItem GetSyncItem(Guid clientRegistrationKey, Guid id)
        {
            var device = devices.GetById(clientRegistrationKey);
            if (device == null)
                throw new ArgumentException("Device was not found.");

            // item could be changed on server and sequence would be different
            var item = storage.GetLatestVersion(id);

            return item;
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