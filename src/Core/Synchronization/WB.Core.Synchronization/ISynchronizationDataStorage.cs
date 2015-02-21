using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.Synchronization
{
    public interface ISynchronizationDataStorage : IReadSideRepositoryCleaner, IChacheableRepositoryWriter
    {
        void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId, DateTime timestamp);
        void SaveQuestionnaire(QuestionnaireDocument doc, long version, bool allowCensusMode, DateTime timestamp);
        void MarkInterviewForClientDeleting(Guid id, Guid? responsibleId, DateTime timestamp);
        void SaveUser(UserDocument doc, DateTime timestamp);
        SyncItem GetLatestVersion(string id);
        IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(string lastSyncedPackageId, Guid userId);
        void DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, DateTime timestamp);
        void SaveQuestionnaireAssembly(Guid publicKey, long version, string assemblyAsBase64String, DateTime timestamp);
        SynchronizationChunkMeta GetChunkInfoByTimestamp(DateTime timestamp, Guid userId);
    }
}