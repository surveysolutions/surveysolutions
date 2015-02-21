using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.Synchronization.MetaInfo;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class SimpleSynchronizationDataStorage : ISynchronizationDataStorage
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStorage;
        private readonly IMetaInfoBuilder metaBuilder;
        private readonly IChunkWriter chunkStorageWriter;
        private readonly IChunkReader chunkStorageReader;

        private readonly IArchiveUtils archiver;

        public static Guid AssemblySeed = new Guid("371EF2E6-BF1D-4E36-927D-2AC13C41EF7B");
        private readonly bool useCompression ;

        public SimpleSynchronizationDataStorage(
            IQueryableReadSideRepositoryReader<UserDocument> userStorage,
            IChunkWriter chunkStorageWriter, IChunkReader chunkStorageReader,
            IMetaInfoBuilder metaBuilder, IArchiveUtils archiver)
        {
            this.userStorage = userStorage;
            this.chunkStorageWriter = chunkStorageWriter;
            this.chunkStorageReader = chunkStorageReader;
            this.metaBuilder = metaBuilder;
            this.archiver = archiver;
            this.useCompression = true;
        }

        public void Clear()
        {
            chunkStorageWriter.Clear();
        }

        public void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId, DateTime timestamp)
        {
            var syncItem = CreateSyncItem(doc.Id, SyncItemType.Questionnare, GetItemAsContent(doc), GetItemAsContent(metaBuilder.GetInterviewMetaInfo(doc)));

            chunkStorageWriter.StoreChunk(syncItem, responsibleId, timestamp);
        }

        private SyncItem CreateSyncItem(Guid id, string itemType, string rawContent, string rawMeta)
        {
            var content = useCompression ? archiver.CompressString(rawContent) : rawContent;
            var metaInfo = (useCompression && !string.IsNullOrWhiteSpace(rawMeta)) ? archiver.CompressString(rawMeta) : rawMeta;

            return new SyncItem
            {
                RootId = id,
                ItemType = itemType,
                IsCompressed = useCompression,
                Content = content,
                MetaInfo = metaInfo 
            };
        }

        public void MarkInterviewForClientDeleting(Guid id, Guid? responsibleId, DateTime timestamp)
        {
            var syncItem = CreateSyncItem(id, SyncItemType.DeleteQuestionnare, id.ToString(), string.Empty);

            chunkStorageWriter.StoreChunk(syncItem, responsibleId, timestamp);
        }

        public void SaveQuestionnaire(QuestionnaireDocument doc, long version, bool allowCensusMode, DateTime timestamp)
        {
            doc.IsDeleted = false;

            var questionnaireMetadata = new QuestionnaireMetadata(doc.PublicKey, version, allowCensusMode);
            var syncItem = CreateSyncItem(doc.PublicKey.Combine(version), SyncItemType.Template, GetItemAsContent(doc), 
                GetItemAsContent(questionnaireMetadata));

            chunkStorageWriter.StoreChunk(syncItem, null, timestamp);
        }

        public void DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, DateTime timestamp)
        {
            var questionnaireMetadata = new QuestionnaireMetadata(questionnaireId, questionnaireVersion, false);

            var syncItem = CreateSyncItem(questionnaireId.Combine(questionnaireVersion), SyncItemType.DeleteTemplate, questionnaireId.ToString(), 
                GetItemAsContent(questionnaireMetadata));
            
            chunkStorageWriter.StoreChunk(syncItem, null, timestamp);
        }

        public void SaveUser(UserDocument doc, DateTime timestamp)
        {
            if (doc.Roles.Contains(UserRoles.Operator))
            {
                SaveInteviewer(doc, timestamp);
            }
        }
       
        public SyncItem GetLatestVersion(string id)
        {
            var result = chunkStorageReader.ReadChunk(id);
            return result;
        }

        public IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(string lastSyncedPackageId, Guid userId)
        {
            var users = GetUserTeamates(userId);
            return chunkStorageReader.GetChunkMetaDataCreatedAfter(lastSyncedPackageId, users);
        }

        private IEnumerable<Guid> GetUserTeamates(Guid userId)
        {
            var user = userStorage.Query(_ => _.Where(u => u.PublicKey == userId)).ToList().FirstOrDefault();
            if (user == null)
                return Enumerable.Empty<Guid>();

            Guid supervisorId = user.Roles.Contains(UserRoles.Supervisor) ? userId : user.Supervisor.Id;

            var team =
                userStorage.QueryAll(u => u.Supervisor != null && u.Supervisor.Id == supervisorId)
                    .Select(u => u.PublicKey)
                    .ToList();
            team.Add(supervisorId);
            return team;
        }


        private void SaveInteviewer(UserDocument doc, DateTime timestamp)
        {
            var syncItem = CreateSyncItem(doc.PublicKey, SyncItemType.User, GetItemAsContent(doc), string.Empty);

            chunkStorageWriter.StoreChunk(syncItem, doc.PublicKey, timestamp);
        }


        public void SaveQuestionnaireAssembly(Guid publicKey, long version, string assemblyAsBase64String, DateTime timestamp)
        {
            var meta = new QuestionnaireAssemblyMetadata(publicKey, version);

            var syncItem = CreateSyncItem(publicKey.Combine(AssemblySeed).Combine(version), SyncItemType.QuestionnaireAssembly,
                GetItemAsContent(assemblyAsBase64String ?? string.Empty), GetItemAsContent(meta));

            chunkStorageWriter.StoreChunk(syncItem, null, timestamp);
        }

        public SynchronizationChunkMeta GetChunkInfoByTimestamp(DateTime timestamp, Guid userId)
        {
            var users = GetUserTeamates(userId);
            return this.chunkStorageReader.GetChunkMetaDataByTimestamp(timestamp, users);
        }

        private static string GetItemAsContent(object item)
        {
            var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All, 
                    NullValueHandling = NullValueHandling.Ignore
                };

            return  JsonConvert.SerializeObject(item, Formatting.None, settings);
        }

        public void EnableCache()
        {
            chunkStorageWriter.EnableCache();
        }

        public void DisableCache()
        {
            chunkStorageWriter.DisableCache();
        }

        public string GetReadableStatus()
        {
            return chunkStorageWriter.GetReadableStatus();
        }

        public Type ViewType { get { return chunkStorageWriter.ViewType; } }
    }
}
