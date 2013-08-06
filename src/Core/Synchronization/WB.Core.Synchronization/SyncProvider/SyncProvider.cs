using System.Collections.Generic;
using Main.Core.Commands.Sync;
using Main.Core;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;
    using Newtonsoft.Json;
    
    using Main.Core.Events;

    internal class SyncProvider : ISyncProvider
    {
        private ICommandService commandService = NcqrsEnvironment.Get<ICommandService>();
        
        #warning ViewFactory should be used here
        private readonly IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices;

        private readonly ISynchronizationDataStorage storage;
        private readonly IIncomePackagesRepository incomeRepository;

        private readonly ILogger logger;


        public SyncProvider(IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices,
            ISynchronizationDataStorage storage,IIncomePackagesRepository incomeRepository,
            ILogger logger)
        {
            this.devices = devices;
            this.incomeRepository = incomeRepository;
            this.storage = storage;
            this.logger = logger;

        }

        public SyncItem GetSyncItem(Guid clientRegistrationKey, Guid id, long sequence)
        {
            var device = devices.GetById(clientRegistrationKey);
            if (device == null)
                throw new ArgumentException("Device was not found.");

            // item could be changed on server and sequence would be different
            
            var item = storage.GetLatestVersion(id);
            
            
            return item;
        }

        public IEnumerable<SyncItem> GetSyncItemBulk(Guid userId, Guid clientRegistrationKey, long sequence)
        {
            var device = devices.GetById(clientRegistrationKey);
            if (device == null)
                throw new ArgumentException("Device was not found.");



            throw new NotImplementedException();
        }
        
        public IEnumerable<Guid> GetAllARIds(Guid userId, Guid clientRegistrationKey)
        {
            var device = devices.GetById(clientRegistrationKey);
            //return storate.GetChunksCreatedAfter(0, userId);

            if (device == null)
                throw new ArgumentException("Device was not found.");
           
            return storage.GetChunksCreatedAfter(device.LastSyncItemIdentifier, userId);
        }

        public IEnumerable<KeyValuePair<long, Guid>> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, long clientSequence)
        {
            var device = devices.GetById(clientRegistrationKey);

            if (device == null)
                throw new ArgumentException("Device was not found.");

            /*if (clientSequence != device.LastSyncItemIdentifier)
                logger.Info(string.Format("Local [{0}] and remote [{1}] sequence number mismatch.", device.LastSyncItemIdentifier, clientSequence));*/

            return storage.GetChunkPairsCreatedAfter(clientSequence, userId);
        }


        public HandshakePackage CheckAndCreateNewSyncActivity(ClientIdentifier identifier)
        {
            Guid ClientRegistrationKey;
            
            //device verification
            ClientDeviceDocument device = null;
            if (identifier.ClientRegistrationKey.HasValue)
            {
                if (identifier.ClientRegistrationKey.Value == Guid.Empty)
                    throw new ArgumentException("Unknown device.");

                device = devices.GetById(identifier.ClientRegistrationKey.Value);
                if (device == null)
                {
                    //keys were provided but we can't find device
                    //probably device was init with other supervisor 
                    throw new ArgumentException("Unknown device.");
                }

                //TODO: check device validity

                ClientRegistrationKey = device.PublicKey;
            }
            else //register new device
            {
                ClientRegistrationKey = Guid.NewGuid();

                commandService.Execute(new CreateClientDeviceCommand(ClientRegistrationKey, identifier.ClientDeviceKey, identifier.ClientInstanceKey));
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