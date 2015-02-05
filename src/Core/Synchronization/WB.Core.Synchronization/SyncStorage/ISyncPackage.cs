using System;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface ISyncPackage : IIndexedView
    {
        string PackageId { get; }

        DateTime Timestamp { get; }
    }
}