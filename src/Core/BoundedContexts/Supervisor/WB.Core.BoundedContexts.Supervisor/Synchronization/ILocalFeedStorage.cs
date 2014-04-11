using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ILocalFeedStorage
    {
        LocalUserChangedFeedEntry GetLastEntry();
        void Store(IEnumerable<LocalUserChangedFeedEntry> userChangedEvent);
        IEnumerable<LocalUserChangedFeedEntry> GetNotProcessedSupervisorRelatedEvents(string supervisorId);
        void Store(LocalUserChangedFeedEntry userChangedEvent);
    }
}