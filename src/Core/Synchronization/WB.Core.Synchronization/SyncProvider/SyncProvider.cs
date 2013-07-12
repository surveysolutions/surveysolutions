using System.Collections.Generic;
using Main.Core.Commands.Sync;
using Main.Core;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

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

        private readonly ILogger logger;

        public SyncProvider(IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices,
            ISynchronizationDataStorage storage,
            ILogger logger)
        {
            this.devices = devices;
            //this.syncActivities = syncActivities;
            this.storage = storage;
            this.logger = logger;
        }

        public SyncItem GetSyncItem(Guid clientRegistrationKey, Guid id, long sequence)
        {
            var device = devices.GetById(clientRegistrationKey);
            if (device == null)
                throw new ArgumentException("Device was not found.");

            var item = storage.GetLatestVersion(id);
            //doing tricky thing
            //we are saving old sequence even if new version was returned
            commandService.Execute(new UpdateClientDeviceLastSyncItemCommand(clientRegistrationKey, sequence));
            
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

            if (clientSequence != device.LastSyncItemIdentifier)
                logger.Info(string.Format("Local [{0}] and remote [{1}] sequence number mismatch.", device.LastSyncItemIdentifier, clientSequence));

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
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");
            
            var items = GetContentAsItem<AggregateRootEvent[]>(item);

            var processor = new SyncEventHandler();
            processor.Merge(items);
            processor.Commit();
            
            return true;
        }

        private T GetContentAsItem<T>(SyncItem syncItem)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var item = JsonConvert.DeserializeObject<T>(
                syncItem.IsCompressed ?
                PackageHelper.DecompressString(syncItem.Content) :
                syncItem.Content, 
                settings);

            return item;
        }

    }
}