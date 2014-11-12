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
    public interface ISynchronizationDataStorage : IReadSideRepositoryCleaner, IReadSideRepositoryWriter
    {
        void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId, DateTime timestamp);
        void SaveQuestionnaire(QuestionnaireDocument doc, long version, bool allowCensusMode, DateTime timestamp);
        void MarkInterviewForClientDeleting(Guid id, Guid? responsibleId, DateTime timestamp);
        void SaveImage(Guid publicKey, string title, string desc, string origData, DateTime timestamp);
        void SaveUser(UserDocument doc, DateTime timestamp);
        SyncItem GetLatestVersion(Guid id);
        IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(DateTime timestamp, Guid userId);
        void DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, DateTime timestamp);
        void SaveTemplateAssembly(Guid publicKey, long version, string assemblyAsBase64String, DateTime timestamp);
    }
}