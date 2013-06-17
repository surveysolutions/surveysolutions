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
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnaires;

        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        private IQueryableDenormalizerStorage<ClientDeviceDocument> devices;

        private ISynchronizationDataStorage storate;

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

        public SyncProvider(IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnaires,
                            IQueryableDenormalizerStorage<UserDocument> users,
                            IQueryableDenormalizerStorage<ClientDeviceDocument> devices,
                            ISynchronizationDataStorage storate)
        {
            this.questionnaires = questionnaires;
            this.users = users;
            this.devices = devices;
            this.storate = storate;
        }

        public SyncItem GetSyncItem(Guid id)
        {
            return storate.GetLatestVersion(id);
        }

        public IEnumerable<SyncItemsMeta> GetAllARIds(Guid userId)
        {
            var result = new List<SyncItemsMeta>();

            List<Guid> users = GetUsers(userId);
            result.AddRange(users.Select(i => new SyncItemsMeta(i, SyncItemType.User, null)));

            List<Guid> questionnaires = GetQuestionnaires(users);
            result.AddRange(questionnaires.Select(i => new SyncItemsMeta(i, SyncItemType.Questionnare, null)));
         

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
            var supervisor =
                users.Query<UserLight>(_ => _.Where(u => u.PublicKey == userId).Select(u => u.Supervisor).FirstOrDefault());
            if (supervisor == null)
                throw new ArgumentException("user is absent");
            return
                 this.users.Query<List<Guid>>(_ => _
                     .Where(t => t.Supervisor != null && t.Supervisor.Id == supervisor.Id)
                     .Select(u => u.PublicKey)
                     .ToList());

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
