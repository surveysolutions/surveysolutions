using System;
using Main.Core.Events;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android.Core.Model
{
    public interface IChangeLogStore:IBackupable
    {
        void SaveChangeset(AggregateRootEvent[] recordData, Guid recordId);
        string GetChangesetContent(Guid recordId);
        void DeleteDraftChangeSet(Guid recordId);
    }
}