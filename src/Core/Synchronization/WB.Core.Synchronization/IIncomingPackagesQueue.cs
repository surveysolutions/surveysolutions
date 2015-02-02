using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface IIncomingPackagesQueue
    {
        void PushSyncItem(string item);
        int QueueLength { get; }
        void DeQueue();
        IEnumerable<Guid> GetListOfUnhandledPackages();
        string GetUnhandledPackagePath(Guid id);
    }
}  