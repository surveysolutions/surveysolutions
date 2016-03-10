using System;

namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void Enqueue(Guid interviewId, string item);
        int QueueLength { get; }
        string DeQueue();
        void DeleteSyncItem(string syncItemPath);
        IncomingSyncPackage GetSyncItem(string syncItemPath);
        bool HasPackagesByInterviewId(Guid interviewId);
    }
}
