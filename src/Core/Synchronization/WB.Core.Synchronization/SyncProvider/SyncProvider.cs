using System.Collections.Generic;
using System.Linq;
using Main.Core;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.View.CompleteQuestionnaire;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;
    using Newtonsoft.Json;
    using SynchronizationMessages.Synchronization;

    public class SyncProvider : ISyncProvider
    {
        private const bool UseCompression = true;

        //compressed content could be larger than uncompressed for small items 
        private int limitLengtForCompression = 0;

        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnaires;

        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        private IQueryableDenormalizerStorage<ClientDeviceDocument> devices;

        

        public SyncProvider(
            IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> surveys,
            IQueryableDenormalizerStorage<UserDocument> users,
            IQueryableDenormalizerStorage<ClientDeviceDocument> devices
            )
        {
            this.questionnaires = surveys;
            this.users = users;
            this.devices = devices;
        }

        public SyncItem GetSyncItem(Guid id, string type)
        {
            switch (type)
            {
                case SyncItemType.File:
                    return null; // todo: file support
                    break;
                case SyncItemType.Questionnare:
                    return GetItem(CreateQuestionnarieDocument(id), id, type);
                    break;
                case SyncItemType.User:
                    return GetItem(this.users.GetById(id), id, type);
                    break;
                default:
                    return null;
            }
        }

        public IEnumerable<SyncItemsMeta> GetAllARIds(Guid userId)
        {
            var result = new List<SyncItemsMeta>();

            List<Guid> users = GetUsers(userId);
            result.AddRange(users.Select(i => new SyncItemsMeta(i, SyncItemType.User, null)));

            List<Guid> questionnaires = GetQuestionnaires(users);
            result.AddRange(questionnaires.Select(i => new SyncItemsMeta(i, SyncItemType.Questionnare, null)));
            /*
                        //temporary disabled due to non support in android app
                        List<Guid> files = GetFiles();
                        result.AddRange(files.Select(i => new SyncItemsMeta(i, SyncItemType.File, null)));
            */

            return result;
        }


        private List<Guid> GetQuestionnaires(List<Guid> users)
        {
            var listOfStatuses = SurveyStatus.StatusAllowDownSupervisorSync();
            return this.questionnaires.Query<List<Guid>>(_ => _
                                                                  .Where(q => q.Status.PublicId.In(listOfStatuses)
                                                                              && q.Responsible != null &&
                                                                              q.Responsible.Id.In(users))
                                                                  .Select(i => i.PublicKey)
                                                                  .ToList());
        }

        private List<Guid> GetUsers(Guid userId)
        {
            var supervisorId =
                users.Query<Guid>(_ => _.Where(u => u.PublicKey == userId).Select(u => u.Supervisor.Id).FirstOrDefault());
            if (supervisorId == null)
                throw new ArgumentException("user is absent");
            return
                 this.users.Query<List<Guid>>(_ => _
                     .Where(t => t.Supervisor != null && t.Supervisor.Id == supervisorId)
                     .Select(u => u.PublicKey)
                     .ToList());

        }

        public Guid CheckAndCreateNewProcess(ClientIdentifier identifier)
        {
            ClientDeviceDocument device = null;
            if (identifier.ClientKey.HasValue || identifier.ClientKey!=Guid.Empty)
            {
                device = devices.GetById(identifier.ClientKey.Value);
            }

            if (device == null)
            {
                //create new device
            }

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
