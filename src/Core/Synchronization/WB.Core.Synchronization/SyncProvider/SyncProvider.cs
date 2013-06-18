using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Commands.Sync;
using Main.Core;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide;
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
        private ICommandService commandService = NcqrsEnvironment.Get<ICommandService>();
        
        #warning ViewFactory should be used here
        private readonly IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> questionnaires;

        #warning ViewFactory should be used here
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        #warning ViewFactory should be used here
        private readonly IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices;

        private ISynchronizationDataStorage storage;

        private IQueryableReadSideRepositoryReader<SyncActivityDocument> syncActivities;

        public SyncProvider(
            IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> questionnaires,
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices,
            IQueryableReadSideRepositoryReader<SyncActivityDocument> syncActivities,
            ISynchronizationDataStorage storage)
        {
            this.questionnaires = questionnaires;
            this.users = users;
            this.devices = devices;
            this.syncActivities = syncActivities;
            this.storage = storage;
        }

        public SyncItem GetSyncItem(Guid deviceId, Guid id)
        {
            var item = storage.GetLatestVersion(id);
            commandService.Execute(new UpdateClientDeviceLastSyncItemCommand(deviceId, item.ChangeTracker));
            //commandService.Execute(new UpdateSyncActivityCommand(syncActivityId, item.LastChangeDate, id));
            return item;
        }

        public IEnumerable<SyncItemsMeta> GetAllARIds(Guid userId, Guid clientRegistrationKey)
        {
           /* var result = new List<SyncItemsMeta>();

            var device = devices.GetById(clientRegistrationKey);

            if(device == null)
                throw new ArgumentException("Syncronization activity was not found.");

            //DateTime syncPoint = activity.LastChangeDate;

            List<Guid> users = GetUsers(userId, device.LastSyncItemIdentifier);
            result.AddRange(users.Select(i => new SyncItemsMeta(i, SyncItemType.User, null)));

            List<Guid> questionnaires = GetQuestionnaires(users, device.LastSyncItemIdentifier);
            result.AddRange(questionnaires.Select(i => new SyncItemsMeta(i, SyncItemType.Questionnare, null)));
         

            return result;*/
            return storate.GetChunksCreatedAfter(0);
        }


        private IEnumerable<Guid> GetQuestionnaires(IEnumerable<Guid> users, long lastSyncItemIdentifier)
        {
            var listOfStatuses = SurveyStatus.StatusAllowDownSupervisorSync();
            return this.questionnaires.Query<List<Guid>>(_ => _
                                                                  .Where(q => q.Status.PublicId.In(listOfStatuses)
                                                                              && q.Responsible != null &&
                                                                              q.Responsible.Id.In(users) /*&& 
                                                                              q.LastEntryDate > lastChangeDate*/)
                                                                  .Select(i => i.PublicKey)
                                                                  .ToList());
        }

        private List<Guid> GetUsers(Guid userId, long lastSyncItemIdentifier)
        {
            var supervisor =
                users.Query<UserLight>(_ => _.Where(u => u.PublicKey == userId).Select(u => u.Supervisor).FirstOrDefault());
            if (supervisor == null)
                throw new ArgumentException("user is absent");
            return
                 this.users.Query<List<Guid>>(_ => _
                     .Where(t => t.Supervisor != null && t.Supervisor.Id == supervisor.Id /*&& t.LastChangeDate > LastChangeDate*/)
                     .Select(u => u.PublicKey)
                     .ToList());
        }

        public HandshakePackage CheckAndCreateNewSyncActivity(ClientIdentifier identifier)
        {
            Guid deviceId;
            
            //device verification
            ClientDeviceDocument device = null;
            if (identifier.ClientRegistrationKey.HasValue || identifier.ClientRegistrationKey != Guid.Empty)
            {
                device = devices.GetById(identifier.ClientRegistrationKey.Value);
                if (device == null)
                {
                    //keys were provided but we can't find device
                    throw new InvalidDataException("Unknown device.");
                }

                //TODO: check device validity


                deviceId = identifier.ClientRegistrationKey.Value;
            }
            else //register new device
            {
                deviceId = Guid.NewGuid();
                
                commandService.Execute(new CreateClientDeviceCommand(deviceId, identifier.ClientDeviceKey, identifier.ClientInstanceKey));
            }

            Guid syncActivityKey = CreateSyncActivity(deviceId);

            return new HandshakePackage(identifier.ClientInstanceKey, syncActivityKey, deviceId);
        }
        /*
        private Guid HandleDevice(ClientIdentifier identifier)
        {

        }*/

        private Guid CreateSyncActivity(Guid deviceId)
        {
            Guid syncActivityId = Guid.NewGuid();
            commandService.Execute(new CreateSyncActivityCommand(syncActivityId, deviceId));
            return syncActivityId;
        }

        public bool HandleSyncItem(SyncItem item, Guid syncActivityId)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");
            if (Guid.Empty == syncActivityId)
                throw new ArgumentException("Sync Activity Identifier is not set.");

            //check and validate sync activity

            var items = GetContentAsItem<AggregateRootEvent[]>(item);

            var processor = new SyncEventHandler();
            processor.Merge(items);
            processor.Commit();

            //commandService.Execute(new UpdateSyncActivityCommand(syncActivityId));

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