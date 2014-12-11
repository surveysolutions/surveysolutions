﻿using System.Collections.Generic;
using Main.Core.Commands.Sync;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Resources;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;

    internal class SyncProvider : ISyncProvider
    {
        private readonly ICommandService commandService = NcqrsEnvironment.Get<ICommandService>();
        
        private readonly IQueryableReadSideRepositoryWriter<ClientDeviceDocument> devices;

        private readonly ISynchronizationDataStorage storage;
        private readonly IIncomePackagesRepository incomeRepository;

        private readonly ILogger logger;


        public SyncProvider(IQueryableReadSideRepositoryWriter<ClientDeviceDocument> devices,
            ISynchronizationDataStorage storage,IIncomePackagesRepository incomeRepository,
            ILogger logger)
        {
            this.devices = devices;
            this.incomeRepository = incomeRepository;
            this.storage = storage;
            this.logger = logger;

        }

        public SyncItem GetSyncItem(Guid clientRegistrationKey, Guid id, DateTime timestamp)
        {
            var device = devices.GetById(clientRegistrationKey);
            if (device == null)
                throw new ArgumentException("Device was not found.");

            // item could be changed on server and sequence would be different
            
            var item = storage.GetLatestVersion(id);
            
            
            return item;
        }

        public IEnumerable<SyncItem> GetSyncItemBulk(Guid userId, Guid clientRegistrationKey, DateTime timestamp)
        {
            var device = devices.GetById(clientRegistrationKey);
            if (device == null)
                throw new ArgumentException("Device was not found.");



            throw new NotImplementedException();
        }

        public IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, DateTime timestamp)
        {
            var device = devices.GetById(clientRegistrationKey);

            if (device == null)
                throw new ArgumentException("Device was not found.");

            /*if (clientSequence != device.LastSyncItemIdentifier)
                logger.Info(string.Format("Local [{0}] and remote [{1}] sequence number mismatch.", device.LastSyncItemIdentifier, clientSequence));*/

            return storage.GetChunkPairsCreatedAfter(timestamp, userId);
        }


        public HandshakePackage CheckAndCreateNewSyncActivity(ClientIdentifier identifier)
        {
            Guid ClientRegistrationKey;
            
            //device verification
            ClientDeviceDocument device = null;
            if (identifier.ClientRegistrationKey.HasValue)
            {
                if (identifier.ClientRegistrationKey.Value == Guid.Empty)
                    throw new ArgumentException(SyncProviderMessages.Client_registration_key_can_t_be_empty);

                device = devices.GetById(identifier.ClientRegistrationKey.Value);
                if (device == null)
                {
                    //keys were provided but we can't find device
                    //probably device has been synchronized with other supervisor application

                    throw new Exception(SyncProviderMessages.Device_was_not_found__It_could_be_linked_to_other_system);
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
                    throw new Exception(SyncProviderMessages.Device_was_assigned_to_another_Supervisor);
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

        public bool HandleSyncItem(SyncItem item, Guid syncActivityId)
        {
            if (item == null)
                throw new ArgumentException("Sync Item is not set.");

            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item content is not set.");

            if (item.Id == Guid.Empty)
                throw new ArgumentException("Sync Item id is not set.");


            incomeRepository.StoreIncomingItem(item);
            return true;

        }



    }
}