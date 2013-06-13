using System;
using Main.Core.Events;

namespace CAPI.Android.Core.Model.ChangeLog
{
    public interface IChangeLogStore
    {
        void SaveChangeset(AggregateRootEvent[] recordData, Guid recordId);
        string GetChangesetContent(Guid recordId);
        void DeleteDraftChangeSet(Guid recordId);
    }
}