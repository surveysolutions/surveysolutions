using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.SyncPackageApplier
{
    public interface ISyncPackageApplier
    {
        bool CheckAndApplySyncPackage(Guid id);
    }
}