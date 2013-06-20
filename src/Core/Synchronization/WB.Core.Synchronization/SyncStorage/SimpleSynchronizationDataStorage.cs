using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Synchronization;
using Newtonsoft.Json;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SimpleSynchronizationDataStorage : ISynchronizationDataStorage
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;

        private readonly IChunkStorage chunkStorage;

        private const bool UseCompression = true;

        public SimpleSynchronizationDataStorage(
            IQueryableReadSideRepositoryReader<UserDocument> userStorage, 
            IChunkStorage chunkStorage
            )
        {
            this.userStorage = userStorage;
            this.chunkStorage = chunkStorage;
        }

        public void SaveQuestionnarie(CompleteQuestionnaireStoreDocument doc, Guid responsibleId)
        {
            var syncItem = new SyncItem
                {
                    Id = doc.PublicKey,
                    ItemType = SyncItemType.Questionnare,
                    IsCompressed = UseCompression,
                    Content = GetItemAsContent(CreateQuestionnarieDocument(doc))
                };
            chunkStorage.StoreChunk(syncItem, responsibleId);
        }

        public void DeleteQuestionnarie(Guid id, Guid responsibleId)
        {
            var syncItem = new SyncItem
            {
                Id = id,
                ItemType = SyncItemType.DeleteQuestionnare,
                IsCompressed = UseCompression,
                Content = id.ToString()
            };
            chunkStorage.StoreChunk(syncItem, responsibleId);
        }

        public void SaveImage(Guid publicKey, string title, string desc, string origData)
        {
            var fileDescription = new FileSyncDescription()
                {
                    Description = desc,
                    OriginalFile = origData,
                    PublicKey = publicKey,
                    Title = title
                };
            var syncItem = new SyncItem
            {
                Id = publicKey,
                ItemType = SyncItemType.File,
                IsCompressed = UseCompression,
                Content = GetItemAsContent(fileDescription)
            };
            chunkStorage.StoreChunk(syncItem, null);
        }

        public void SaveUser(UserDocument doc)
        {
            if (doc.Roles.Contains(UserRoles.Operator))
            {
                SaveInteviewer(doc);
            }
        }
       
        public SyncItem GetLatestVersion(Guid id)
        {
            var result = chunkStorage.ReadChunk(id);
            return result;
        }

        public IEnumerable<Guid> GetChunksCreatedAfter(long sequence, Guid userId)
        {
            var users = GetUserTeamates(userId);
            return
                chunkStorage.GetChunksCreatedAfterForUsers(sequence, users);
                }
        public IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(long sequence)
        {
            return fileChunkStorage.GetChunkPairsCreatedAfter(sequence);
        }

        private IEnumerable<Guid> GetUserTeamates(Guid userId)
        {
            var user = userStorage.Query(_ => _.Where(u => u.PublicKey == userId).ToList().FirstOrDefault());
            if (user == null)
                return Enumerable.Empty<Guid>();

            Guid supervisorId = user.Roles.Contains(UserRoles.Supervisor) ? userId : user.Supervisor.Id;

            var team=
                userStorage.Query(
                    _ => _.Where(u => u.Supervisor != null && u.Supervisor.Id == supervisorId).Select(u => u.PublicKey)).ToList();
            team.Add(supervisorId);
            return team;
        }


        private void SaveInteviewer(UserDocument doc)
        {
            var syncItem = new SyncItem
            {
                Id = doc.PublicKey,
                ItemType = SyncItemType.User,
                IsCompressed = UseCompression,
                Content = GetItemAsContent(doc)
            };

            chunkStorage.StoreChunk(syncItem, doc.PublicKey);
        }

       

        #region from sync provider


        private CompleteQuestionnaireDocument CreateQuestionnarieDocument(CompleteQuestionnaireStoreDocument data)
        {
            var retval = new CompleteQuestionnaireDocument();

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
            return  JsonConvert.SerializeObject(item, Formatting.None, settings);
        }
        #endregion
    }
}
