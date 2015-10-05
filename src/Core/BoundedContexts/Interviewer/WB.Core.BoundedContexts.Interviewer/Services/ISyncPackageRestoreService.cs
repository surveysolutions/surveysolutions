using System;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ISyncPackageRestoreService
    {
        void CheckAndApplySyncPackage(Guid id);
    }
}