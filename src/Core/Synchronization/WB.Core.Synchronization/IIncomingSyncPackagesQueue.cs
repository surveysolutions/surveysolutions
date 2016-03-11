using System;
using System.Collections.Generic;

namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void Enqueue(Guid interviewId, string item);
        int QueueLength { get; }
        string DeQueue(int skip);
        void DeleteSyncItem(string syncItemPath);
        IncomingSyncPackage GetSyncItem(string syncItemPath);
        bool HasPackagesByInterviewId(Guid interviewId);
        List<string> DeQueueMany(int count);
    }
}
