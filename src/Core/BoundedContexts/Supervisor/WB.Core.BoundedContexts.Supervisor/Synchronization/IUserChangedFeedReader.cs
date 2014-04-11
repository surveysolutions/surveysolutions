using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface IUserChangedFeedReader 
    {
        Task<List<LocalUserChangedFeedEntry>> ReadAfterAsync(LocalUserChangedFeedEntry lastStoredFeedEntry);
    }
}