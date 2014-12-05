using System;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ISyncPackageRestoreService
    {
        bool CheckAndApplySyncPackage(Guid id);
    }
}