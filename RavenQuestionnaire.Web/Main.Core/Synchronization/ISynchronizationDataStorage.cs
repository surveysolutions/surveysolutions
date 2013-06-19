using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace Main.Core.Synchronization
{
    public interface ISynchronizationDataStorage
    {
        void SaveQuestionnarie(CompleteQuestionnaireStoreDocument doc, Guid responsibleId);
        void DeleteQuestionnarie(Guid id, Guid responsibleId);
        void SaveImage(Guid publicKey, string title, string desc, string origData);
        void SaveUser(UserDocument doc);
        SyncItem GetLatestVersion(Guid id);
        IEnumerable<Guid> GetChunksCreatedAfter(long sequence, Guid userId);
    }
}