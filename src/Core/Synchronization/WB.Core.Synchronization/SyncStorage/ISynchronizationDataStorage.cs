using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface ISynchronizationDataStorage
    {
        void SaveQuestionnarie(Guid id);
        void SaveUser(UserDocument doc);
        SyncItem GetLatestVersion(Guid id);
        IEnumerable<Guid> GetChunksCreatedAfter(long sequence);
    }
}