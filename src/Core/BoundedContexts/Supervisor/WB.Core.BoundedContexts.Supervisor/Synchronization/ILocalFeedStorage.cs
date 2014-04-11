using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ILocalFeedStorage
    {
        Task<UserChangedFeedEntry> GetLastEntryAsync();
        void Store(UserChangedFeedEntry userChangedEvent);
    }
}