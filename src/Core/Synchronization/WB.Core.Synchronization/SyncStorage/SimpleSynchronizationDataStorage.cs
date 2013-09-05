using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.Synchronization.MetaInfo;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class SimpleSynchronizationDataStorage : ISynchronizationDataStorage
    {
        private readonly IQueryableReadSideRepositoryWriter<UserDocument> userStorage;
        private readonly IMetaInfoBuilder metaBuilder;
        private readonly IChunkWriter chunkStorageWriter;
        private readonly IChunkReader chunkStorageReader;

        private const bool UseCompression = true;
        private const bool UseCompressionForFiles = false;

        public SimpleSynchronizationDataStorage(
            IQueryableReadSideRepositoryWriter<UserDocument> userStorage,
            IChunkWriter chunkStorageWriter, IChunkReader chunkStorageReader, IMetaInfoBuilder metaBuilder
            )
        {
            this.userStorage = userStorage;
            this.chunkStorageWriter = chunkStorageWriter;
            this.chunkStorageReader = chunkStorageReader;
            this.metaBuilder = metaBuilder;
        }

        public void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId)
        {
            var syncItem = new SyncItem
                {
                    Id = doc.Id,
                    ItemType = SyncItemType.Questionnare,
                    IsCompressed = UseCompression,
                    Content = GetItemAsContent(doc),
                    MetaInfo = GetItemAsContent(metaBuilder.GetInterviewMetaInfo(doc)) 
                };
            chunkStorageWriter.StoreChunk(syncItem, responsibleId);
        }

        public void SaveQuestionnaire(QuestionnaireDocument doc)
        {
            var syncItem = new SyncItem
            {
                Id = doc.PublicKey,
                ItemType = SyncItemType.Template,
                IsCompressed = UseCompression,
                Content = GetItemAsContent(doc)
            };
            chunkStorageWriter.StoreChunk(syncItem, null);
        }

        public void MarkInterviewForClientDeleting(Guid id, Guid? responsibleId)
        {
            var syncItem = new SyncItem
            {
                Id = id,
                ItemType = SyncItemType.DeleteQuestionnare,
                IsCompressed = UseCompression,
                Content = id.ToString()
            };
            chunkStorageWriter.StoreChunk(syncItem, responsibleId);
        }

        public void DeleteInterview(Guid id)
        {
            chunkStorageWriter.RemoveChunk(id);
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
                IsCompressed = UseCompressionForFiles,
                Content = GetItemAsContent(fileDescription)
            };
            chunkStorageWriter.StoreChunk(syncItem, null);
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
            var result = chunkStorageReader.ReadChunk(id);
            return result;
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(long sequence, Guid userId)
        {
            var users = GetUserTeamates(userId);
            return
                chunkStorageReader.GetChunkMetaDataCreatedAfter(sequence, users);
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

            chunkStorageWriter.StoreChunk(syncItem, doc.PublicKey);
        }

       

        #region from sync provider

        private string GetItemAsContent(object item)
        {
            var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects, 
                    NullValueHandling = NullValueHandling.Ignore
                };

            return  JsonConvert.SerializeObject(item, Formatting.None, settings);
        }
        #endregion
    }
}
