using System;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ISyncPackageRestoreService
    {
        void CheckAndApplySyncPackage(Guid id);
    }
}