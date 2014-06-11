using System;

namespace WB.Core.BoundedContext.Capi.Synchronization.Synchronization.Cleaner
{
    public interface ICleanUpExecutor {
        void DeleteInterveiw(Guid id);
    }
}