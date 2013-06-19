using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Commands.Sync;
using Main.Core;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;
    using Newtonsoft.Json;
    
    using Main.Core.Events;
    using Infrastructure;

    public class SyncProvider : ISyncProvider
    {


        #warning ViewFactory should be used here
        private readonly IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices;

        private readonly ISynchronizationDataStorage storate;

        public SyncProvider(
            IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices)
        {
            this.devices = devices;
        }

        public SyncProvider(
                            IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices,
                            ISynchronizationDataStorage storate)
        {
            this.devices = devices;
            this.storate = storate;
        }

        public SyncItem GetSyncItem(Guid id, Guid userId)
        {
            return storate.GetLatestVersion(id, userId);
        }

        public IEnumerable<Guid> GetAllARIds(Guid userId)
        {
            return storate.GetChunksCreatedAfter(0, userId);
        }


      

        public Guid CheckAndCreateNewSyncActivity(ClientIdentifier identifier)
        {

            var commandService = NcqrsEnvironment.Get<ICommandService>();
            Guid deviceId;
            //device verification
            ClientDeviceDocument device = null;
            if (identifier.ClientKey.HasValue || identifier.ClientKey != Guid.Empty)
            {
                device = devices.GetById(identifier.ClientKey.Value);
                if (device == null)
                {
                    //keys were provided but we can't find device
                    throw new InvalidDataException("Unknown device.");
                }

                deviceId = identifier.ClientKey.Value;
            }
            else //register new device
            {
                deviceId = Guid.NewGuid();
                
                commandService.Execute(new CreateClientDeviceCommand(deviceId, identifier.ClientDeviceKey, identifier.ClientInstanceKey));
            }


            Guid syncActivityId = Guid.NewGuid();
            commandService.Execute(new CreateSyncActivityCommand(syncActivityId, deviceId));



            throw new NotImplementedException();
        }
        
        public bool HandleSyncItem(SyncItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                return false;

            var items = GetContentAsItem<AggregateRootEvent[]>(item.Content);

            var processor = new SyncEventHandler();
            processor.Merge(items);
            processor.Commit();

            return true;
        }


        private T GetContentAsItem<T>(string content)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            return JsonConvert.DeserializeObject<T>(PackageHelper.DecompressString(content), settings);
        }

    }
}
