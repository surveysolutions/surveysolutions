using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SimpleSynchronizationDataStorage : ISynchronizationDataStorage
    {
        private readonly IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> questionnarieStorage;
       
        private readonly IChunkStorageFactory fileChunkStorageFactory;
        private readonly IDictionary<Guid, List<Guid>> supervisorTeams; 

        public SimpleSynchronizationDataStorage(
            IQueryableReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> questionnarieStorage,
            IChunkStorageFactory fileChunkStorage)
        {
            this.questionnarieStorage = questionnarieStorage;
            this.fileChunkStorageFactory = fileChunkStorage;
            this.supervisorTeams=new Dictionary<Guid, List<Guid>>();
        }

        public void SaveQuestionnarie(Guid id, Guid responsibleId)
        {
            var supervisorid = GetSupervisorByUser(responsibleId);
            if (!supervisorid.HasValue)
                return;
            var syncItem = new SyncItem
            {
                Id = id,
                ItemType = SyncItemType.Questionnare,
                IsCompressed = UseCompression
            };
            GetStorage(supervisorid.Value).StoreChunk(id, GetItemAsContent(syncItem));
        }

        public void DeleteQuestionnarie(Guid id, Guid responsibleId)
        {
            var supervisorid = GetSupervisorByUser(responsibleId);
            if (!supervisorid.HasValue)
                return;

            var syncItem = new SyncItem
            {
                Id = id,
                ItemType = SyncItemType.DeleteQuestionnare,
                Content = id.ToString(),
                IsCompressed = UseCompression
            };
            GetStorage(supervisorid.Value).StoreChunk(id, GetItemAsContent(syncItem));
        }

        public void SaveUser(UserDocument doc)
        {
            if (doc.Roles.Contains(UserRoles.Operator))
            {
                SaveInteviewer(doc);
            }
            if (doc.Roles.Contains(UserRoles.Supervisor))
                SaveSupervisor(doc.PublicKey);
        }
       
        public SyncItem GetLatestVersion(Guid id, Guid userId)
        {
            var supervisorid = GetSupervisorByUser(userId);
            if (!supervisorid.HasValue)
                return null;
            var result = JsonConvert.DeserializeObject<SyncItem>(GetStorage(supervisorid.Value).ReadChunk(id),
                                                           new JsonSerializerSettings
                                                               {
                                                                   TypeNameHandling = TypeNameHandling.Objects
                                                               });

            if (result.ItemType == SyncItemType.Questionnare)
            {
                result.Content = GetItemAsContent(CreateQuestionnarieDocument(id));
            }

            if (UseCompression)
                result.Content = PackageHelper.CompressString(result.Content);

            return result;
        }

        public IEnumerable<Guid> GetChunksCreatedAfter(long sequence, Guid userId)
        {
            var supervisorid = GetSupervisorByUser(userId);
            if (!supervisorid.HasValue)
                return Enumerable.Empty<Guid>();
            return
                GetStorage(supervisorid.Value).GetChunksCreatedAfter(sequence);
        }

        protected IChunkStorage GetStorage(Guid supervisorId)
        {
            return fileChunkStorageFactory.GetStorage(supervisorId);
        }

        private void SaveSupervisor(Guid supervisorId)
        {
            if(supervisorTeams.ContainsKey(supervisorId))
                return;
            supervisorTeams.Add(supervisorId, new List<Guid>());
        }

        private void SaveInteviewer(UserDocument doc)
        {

            var syncItem = new SyncItem
            {
                Id = doc.PublicKey,
                ItemType = SyncItemType.User,
                Content = GetItemAsContent(doc),
                IsCompressed = UseCompression
            };


            GetStorage(doc.Supervisor.Id).StoreChunk(doc.PublicKey, GetItemAsContent(syncItem));


            var supervisorId = GetSupervisorByUser(doc.Supervisor.Id);
            if (!supervisorId.HasValue)
            {
                SaveSupervisor(doc.Supervisor.Id);
            }
            supervisorTeams[doc.Supervisor.Id].Add(doc.PublicKey);
        }

        private Guid? GetSupervisorByUser(Guid userId)
        {
            foreach (var supervisorTeam in supervisorTeams)
            {
                if (supervisorTeam.Value.Contains(userId))
                    return supervisorTeam.Key;
            }
            return null;
        }

        #region from sync provider


      /*  private SyncItem GetItem(object item, Guid id, string type)
        {
            var result = new SyncItem
            {
                Id = id,
                ItemType = type,
                IsCompressed = UseCompression
            };
            if (item != null)
                result.Content = GetItemAsContent(item);
            return result;
        }*/

       

        private CompleteQuestionnaireDocument CreateQuestionnarieDocument(Guid id)
        {
            var retval = new CompleteQuestionnaireDocument();
            var data = this.questionnarieStorage.GetById(id);

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

            return itemToSync;
        }

        private const bool UseCompression = true;

        #endregion
    }
}
