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
        private readonly IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> questionnaires;

        #warning ViewFactory should be used here
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        #warning ViewFactory should be used here
        private readonly IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices;

        private readonly ISynchronizationDataStorage storate;

        public SyncProvider(
            IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> surveys,
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices)
        {
            this.questionnaires = surveys;
            this.users = users;
            this.devices = devices;
        }

        public SyncProvider(IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> questionnaires,
                            IQueryableReadSideRepositoryReader<UserDocument> users,
                            IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices,
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

        public IEnumerable<Guid> GetAllARIds(Guid userId)
        {
           /* var result = new List<SyncItemsMeta>();

            List<Guid> userIds = GetUsers(userId);
            result.AddRange(userIds.Select(i => new SyncItemsMeta(i, SyncItemType.User, null)));

            IEnumerable<Guid> questionnaireIds = GetQuestionnaires(userIds);
            result.AddRange(questionnaireIds.Select(i => new SyncItemsMeta(i, SyncItemType.Questionnare, null)));

            return result;*/
            return storate.GetChunksCreatedAfter(0);
        }


        private IEnumerable<Guid> GetQuestionnaires(IEnumerable<Guid> userId)
        {
            var listOfStatuses = SurveyStatus.StatusAllowDownSupervisorSync();
            return this.questionnaires.Query<List<Guid>>(_ => _
                                                                  .Where(q => q.Status.PublicId.In(listOfStatuses)
                                                                              && q.Responsible != null &&
                                                                              q.Responsible.Id.In(userId))
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
