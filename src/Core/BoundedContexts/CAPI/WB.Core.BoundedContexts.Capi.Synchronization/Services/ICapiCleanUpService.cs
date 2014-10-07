using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Services
{
    public interface ICapiCleanUpService {
        void DeleteInterview(Guid id);
    }
}