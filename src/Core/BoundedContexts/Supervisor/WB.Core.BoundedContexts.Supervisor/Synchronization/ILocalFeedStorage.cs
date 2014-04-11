using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ILocalFeedStorage
    {
        UserChangedFeedEntry GetLastEntry();
        void Store(IEnumerable<UserChangedFeedEntry> userChangedEvent);
    }
}