using System.IO;
using Main.Core.Commands.Sync;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;
    using Newtonsoft.Json;
    using SynchronizationMessages.Synchronization;

    using Main.Core.Events;
    using Main.Synchronization.SycProcessRepository;
    using Infrastructure;

    public class SyncProvider : ISyncProvider
    {
        private const bool UseCompression = true;

        //compressed content could be larger than uncompressed for small items 
        //private int limitLengtForCompression = 0;

        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnaires;

        private readonly IDenormalizerStorage<UserDocument> users;

        private IDenormalizerStorage<ClientDeviceDocument> devices;

        protected readonly ISyncProcessRepository syncProcessRepository;
        

        public SyncProvider(
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> surveys, 
            IDenormalizerStorage<UserDocument> users,
            IDenormalizerStorage<ClientDeviceDocument> devices,
            ISyncProcessRepository syncProcessRepository
            )
        {
            this.questionnaires = surveys;
            this.users = users;
            this.devices = devices;
            this.syncProcessRepository = syncProcessRepository;
        }

        public SyncItem GetSyncItem(Guid id, string type)
        {
            switch (type)
            {
                case SyncItemType.File:
                    return null; // todo: file support
                case SyncItemType.Questionnare:
                    return GetItem(CreateQuestionnarieDocument(id), id, type);
                case SyncItemType.User:
                    return GetItem(this.users.GetById(id), id, type);
                default:
                    return null;
            }
        }

        public Guid CheckAndCreateNewSyncActivity(ClientIdentifier identifier)
        {
            var commandService = NcqrsEnvironment.Get<ICommandService>();

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
            }
            else //register new device
            {
                Guid deviceId = Guid.NewGuid();
                
                commandService.Execute(new CreateClientDeviceCommand(deviceId, identifier.ClientDeviceKey, identifier.ClientInstanceKey));
            }


            Guid syncActivityId = Guid.NewGuid();
            commandService.Execute(new CreateSyncActivityCommand(syncActivityId,));

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

        private CompleteQuestionnaireDocument CreateQuestionnarieDocument(Guid id)
        {
            var retval = new CompleteQuestionnaireDocument();
            var data = this.questionnaires.GetById(id);

            retval.CreatedBy = data.CreatedBy;
            retval.CreationDate = data.CreationDate;
            retval.Creator = data.Creator;
            retval.LastEntryDate = data.LastEntryDate;
            retval.PublicKey = data.PublicKey;
            retval.Responsible = data.Responsible;
            retval.Status = data.Status;
            retval.TemplateId = data.TemplateId;
            retval.Title = data.Title;
            
            retval.Children = data.Children;
            return retval;
        }

        private SyncItem GetItem(object item, Guid id, string type)
        {
            if (item == null)
            {
                return null;
            }

            var result = new SyncItem {Id = id, 
                Content = GetItemAsContent(item), 
                ItemType = type, 
                IsCompressed = UseCompression};

            return result;
        }

        private string GetItemAsContent(object item)
        {
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
            string itemToSync = JsonConvert.SerializeObject(item, Formatting.None, settings);
            
            return UseCompression ? PackageHelper.CompressString(itemToSync) : itemToSync;
        }


        private T GetContentAsItem<T>(string content)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            return JsonConvert.DeserializeObject<T>(PackageHelper.DecompressString(content), settings);
        }

    }
}
