using System;
using Main.Core.Events;
using WB.Core.Infrastructure.Backup;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Core.Model
{
    public interface IChangeLogStore:IBackupable
    {
        void SaveChangeset(AggregateRootEvent[] recordData, Guid recordId);
        SyncItem GetChangesetContent(Guid recordId);
        void DeleteDraftChangeSet(Guid recordId);
    }
}