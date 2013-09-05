using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization
{
    public interface ISynchronizationDataStorage
    {
        void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId);
        void SaveQuestionnaire(QuestionnaireDocument doc);
        void DeleteInterview(Guid id);
        void MarkInterviewForClientDeleting(Guid id, Guid? responsibleId);
        void SaveImage(Guid publicKey, string title, string desc, string origData);
        void SaveUser(UserDocument doc);

        SyncItem GetLatestVersion(Guid id);

        IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(long sequence, Guid userId);

    }
}