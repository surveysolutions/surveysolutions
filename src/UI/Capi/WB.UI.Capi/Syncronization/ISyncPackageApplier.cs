using System;

namespace WB.UI.Capi.Syncronization
{
    public interface ISyncPackageApplier
    {
        bool CheckAndApplySyncPackage(Guid id);
    }
}