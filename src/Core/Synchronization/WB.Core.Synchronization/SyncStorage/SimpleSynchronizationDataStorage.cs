using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
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

        public static Guid AssemblySeed = new Guid("371EF2E6-BF1D-4E36-927D-2AC13C41EF7B");

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

        public void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId, DateTime timestamp)
        {
            var syncItem = new SyncItem
                {
                    Id = doc.Id,
                    ItemType = SyncItemType.Questionnare,
                    IsCompressed = UseCompression,
                    Content = GetItemAsContent(doc),
                    MetaInfo = GetItemAsContent(metaBuilder.GetInterviewMetaInfo(doc)) 
                };
            chunkStorageWriter.StoreChunk(syncItem, responsibleId, timestamp);
        }

        public void MarkInterviewForClientDeleting(Guid id, Guid? responsibleId, DateTime timestamp)
        {
            var syncItem = new SyncItem
            {
                Id = id,
                ItemType = SyncItemType.DeleteQuestionnare,
                IsCompressed = UseCompression,
                Content = id.ToString()
            };
            chunkStorageWriter.StoreChunk(syncItem, responsibleId, timestamp);
        }

        public void SaveQuestionnaire(QuestionnaireDocument doc, long version, bool allowCensusMode, DateTime timestamp)
        {
            doc.IsDeleted = false;
            var syncItem = new SyncItem
            {
                Id = doc.PublicKey.Combine(version),
                ItemType = SyncItemType.Template,
                IsCompressed = UseCompression,
                Content = GetItemAsContent(doc),
                MetaInfo = GetItemAsContent(new QuestionnaireMetadata(doc.PublicKey, version, allowCensusMode)),
            };
            chunkStorageWriter.StoreChunk(syncItem, null, timestamp);
        }

        public void DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, DateTime timestamp)
        {
            var syncItem = new SyncItem
            {
                Id = questionnaireId.Combine(questionnaireVersion),
                ItemType = SyncItemType.DeleteTemplate,
                IsCompressed = UseCompression,
                Content = questionnaireId.ToString(),
                MetaInfo = GetItemAsContent(new QuestionnaireMetadata(questionnaireId, questionnaireVersion, false)),
            };
            chunkStorageWriter.StoreChunk(syncItem, null, timestamp);
        }

        public void SaveImage(Guid publicKey, string title, string desc, string origData, DateTime timestamp)
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
            chunkStorageWriter.StoreChunk(syncItem, null, timestamp);
        }

        public void SaveUser(UserDocument doc, DateTime timestamp)
        {
            if (doc.Roles.Contains(UserRoles.Operator))
            {
                SaveInteviewer(doc, timestamp);
            }
        }
       
        public SyncItem GetLatestVersion(Guid id)
        {
            var result = chunkStorageReader.ReadChunk(id);
            return result;
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(DateTime timestamp, Guid userId)
        {
            var users = GetUserTeamates(userId);
            return
                chunkStorageReader.GetChunkMetaDataCreatedAfter(timestamp, users);
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


        private void SaveInteviewer(UserDocument doc, DateTime timestamp)
        {
            var syncItem = new SyncItem
            {
                Id = doc.PublicKey,
                ItemType = SyncItemType.User,
                IsCompressed = UseCompression,
                Content = GetItemAsContent(doc)
            };

            chunkStorageWriter.StoreChunk(syncItem, doc.PublicKey,timestamp);
        }


        public void SaveTemplateAssembly(Guid publicKey, long version, string assemblyAsBase64String, DateTime timestamp)
        {
            var meta = new QuestionnaireAssemblyMetadata(publicKey, version);

            var syncItem = new SyncItem
            {
                Id = publicKey.Combine(AssemblySeed).Combine(version),
                ItemType = SyncItemType.QuestionnaireAssembly,
                IsCompressed = UseCompressionForFiles,
                Content = assemblyAsBase64String,
                MetaInfo = GetItemAsContent(meta)
            };
            chunkStorageWriter.StoreChunk(syncItem, null, timestamp);
        }

        #region from sync provider

        private static string GetItemAsContent(object item)
        {
            var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All, 
                    NullValueHandling = NullValueHandling.Ignore
                };

            return  JsonConvert.SerializeObject(item, Formatting.None, settings);
        }
        #endregion
    }
}
