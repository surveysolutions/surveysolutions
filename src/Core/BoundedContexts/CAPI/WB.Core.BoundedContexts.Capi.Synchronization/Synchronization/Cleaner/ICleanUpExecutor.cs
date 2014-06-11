using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Cleaner
{
    public interface ICleanUpExecutor {
        void DeleteInterveiw(Guid id);
    }
}