using System;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ICapiCleanUpService
    {
        void DeleteInterview(Guid id);
    }
}