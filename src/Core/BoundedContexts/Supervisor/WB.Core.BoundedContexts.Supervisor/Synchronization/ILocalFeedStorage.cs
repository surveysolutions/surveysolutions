using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ILocalFeedStorage
    {
        LocalUserChangedFeedEntry GetLastEntry();
        void Store(IEnumerable<LocalUserChangedFeedEntry> userChangedEvent);
        IEnumerable<LocalUserChangedFeedEntry> GetNotProcessedSupervisorEvents(string supervisorId);
    }
}