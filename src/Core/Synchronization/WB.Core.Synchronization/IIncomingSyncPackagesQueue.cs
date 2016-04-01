using System;
using System.Collections.Generic;

namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void Enqueue(Guid interviewId, string item);
        int QueueLength { get; }
        IReadOnlyCollection<string> GetTopSyncItemsAsFileNames(int count);
        void DeleteSyncItem(string syncItemPath);
        IncomingSyncPackage GetSyncItem(string syncItemPath);
        bool HasPackagesByInterviewId(Guid interviewId);
    }
}
