using System;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ICapiCleanUpService
    {
        void DeleteInterview(Guid id);

        void DeleteAllInterviewsForUser(Guid userIdAsGuid);
    }
}