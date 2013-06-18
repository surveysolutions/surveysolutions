using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Commands.Sync;
using Main.Core;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Raven.Client.Linq;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;
    using Newtonsoft.Json;
    
    using Main.Core.Events;
    using Infrastructure;

    public class SyncProvider : ISyncProvider
    {
        private const bool UseCompression = true;

        private ICommandService commandService = NcqrsEnvironment.Get<ICommandService>();

        //compressed content could be larger than uncompressed for small items 
        //private int limitLengtForCompression = 0;

        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnaires;

        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        private IQueryableDenormalizerStorage<ClientDeviceDocument> devices;

        private IQueryableDenormalizerStorage<SyncActivityDocument> syncActivities;

        public SyncProvider(
            IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> surveys,
            IQueryableDenormalizerStorage<UserDocument> users,
            IQueryableDenormalizerStorage<ClientDeviceDocument> devices,
            IQueryableDenormalizerStorage<SyncActivityDocument> syncActivities
            )
        {
            this.questionnaires = surveys;
            this.users = users;
            this.devices = devices;
            this.syncActivities = syncActivities;
        }

        public SyncItem GetSyncItem(Guid syncActivityId, Guid id, string type)
        {
            SyncItem item = null;

            switch (type)
            {
                case SyncItemType.File:
                    item = null; // todo: file support
                    break;
                case SyncItemType.Questionnare:
                    item = GetQuestionnarieItem(id);
                    break;
                case SyncItemType.User:
                    item = GetUserItem(id);
                    break;
                default:
                    return null;
            }

            commandService.Execute(new UpdateSyncActivityCommand(syncActivityId, item.LastChangeDate, id));

            return item;
        }

        private SyncItem GetUserItem(Guid id)
        {
            var item = this.users.GetById(id);
            if (item == null)
            {
                return null;
            }

            var result = new SyncItem(id,
                GetItemAsContent(item),
                SyncItemType.User,
                UseCompression,
                item.LastChangeDate);

            return result;
        }

        private SyncItem GetQuestionnarieItem(Guid id)
        {

            var item = CreateQuestionnarieDocument(id);
            if (item == null)
            {
                return null;
            }

            var result = new SyncItem(id,
                GetItemAsContent(item),
                SyncItemType.Questionnare,
                UseCompression,
                item.LastEntryDate);

            return result;
        }

        public IEnumerable<SyncItemsMeta> GetAllARIds(Guid userId, Guid clientRegistrationKey)
        {
            var result = new List<SyncItemsMeta>();

            var activity = syncActivities.GetById(clientRegistrationKey);

            if(activity == null)
                throw new ArgumentException("Syncronization activity was not found.");

            //DateTime syncPoint = activity.LastChangeDate;

            List<Guid> users = GetUsers(userId, activity.LastChangeDate);
            result.AddRange(users.Select(i => new SyncItemsMeta(i, SyncItemType.User, null)));

            List<Guid> questionnaires = GetQuestionnaires(users, activity.LastChangeDate);
            result.AddRange(questionnaires.Select(i => new SyncItemsMeta(i, SyncItemType.Questionnare, null)));
            /*
                        //temporary disabled due to non support in android app
                        List<Guid> files = GetFiles();
                        result.AddRange(files.Select(i => new SyncItemsMeta(i, SyncItemType.File, null)));
            */

            return result;
        }


        private List<Guid> GetQuestionnaires(List<Guid> users, DateTime lastChangeDate)
        {
            var listOfStatuses = SurveyStatus.StatusAllowDownSupervisorSync();
            return this.questionnaires.Query<List<Guid>>(_ => _
                                                                  .Where(q => q.Status.PublicId.In(listOfStatuses)
                                                                              && q.Responsible != null &&
                                                                              q.Responsible.Id.In(users) && 
                                                                              q.LastEntryDate > lastChangeDate)
                                                                  .Select(i => i.PublicKey)
                                                                  .ToList());
        }

        private List<Guid> GetUsers(Guid userId, DateTime LastChangeDate)
        {
            var supervisor =
                users.Query<UserLight>(_ => _.Where(u => u.PublicKey == userId).Select(u => u.Supervisor).FirstOrDefault());
            if (supervisor == null)
                throw new ArgumentException("user is absent");
            return
                 this.users.Query<List<Guid>>(_ => _
                     .Where(t => t.Supervisor != null && t.Supervisor.Id == supervisor.Id && t.LastChangeDate > LastChangeDate)
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
        
        private string GetItemAsContent(object item)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            string itemToSync = JsonConvert.SerializeObject(item, Formatting.None, settings);

            return UseCompression ? PackageHelper.CompressString(itemToSync) : itemToSync;
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