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

        string[] GetListOfUnhandledPackagesForInterview(Guid interviewId);

        IEnumerable<string> GetListOfUnhandledPackages();
        string GetUnhandledPackagePath(string id);
    }
}  