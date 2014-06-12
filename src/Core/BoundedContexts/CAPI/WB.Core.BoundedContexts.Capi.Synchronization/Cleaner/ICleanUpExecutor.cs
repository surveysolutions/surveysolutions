using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Cleaner
{
    public interface ICleanUpExecutor {
        void DeleteInterveiw(Guid id);
    }
}