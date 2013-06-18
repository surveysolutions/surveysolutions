using System.Collections.Generic;
using Main.Core.Events;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;

    public interface ISyncProvider
    {
        SyncItem GetSyncItem(Guid syncId, Guid id);

        IEnumerable<Guid> GetAllARIds(Guid userId, Guid syncActivityId);

        HandshakePackage CheckAndCreateNewSyncActivity(ClientIdentifier identifier);

        bool HandleSyncItem(SyncItem item, Guid syncActivityId);
    }
}
