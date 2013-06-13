using System.Collections.Generic;
using Main.Core.Events;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;

    public interface ISyncProvider
    {
        SyncItem GetSyncItem(Guid id, string type);

        IEnumerable<SyncItemsMeta> GetAllARIds(Guid userId);

        Guid CheckAndCreateNewProcess(ClientIdentifier identifier);

        bool HandleSyncItem(SyncItem item);
    }
}
