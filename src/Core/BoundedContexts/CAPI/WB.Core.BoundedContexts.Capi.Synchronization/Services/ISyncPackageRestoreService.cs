using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Services
{
    public interface ISyncPackageRestoreService
    {
        bool CheckAndApplySyncPackage(Guid id);
    }
}