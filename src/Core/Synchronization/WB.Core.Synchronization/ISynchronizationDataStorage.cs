using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface ISynchronizationDataStorage
    {
        void SaveInterview(CompleteQuestionnaireStoreDocument doc, Guid responsibleId);
        void DeleteInterview(Guid id, Guid responsibleId);
        void SaveImage(Guid publicKey, string title, string desc, string origData);
        void SaveUser(UserDocument doc);
        
        SyncItem GetLatestVersion(Guid id);
        
        IEnumerable<Guid> GetChunksCreatedAfter(long sequence, Guid userId);

        IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(long sequence, Guid userId);
        
    }
}