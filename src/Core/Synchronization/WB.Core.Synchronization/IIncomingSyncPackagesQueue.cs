using System;

namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void Enqueue(Guid interviewId, string item);
        int QueueLength { get; }
        IncomingSyncPackage DeQueue();
        void DeleteSyncItem(string syncItemPath);
        bool HasPackagesByInterviewId(Guid interviewId);
    }
}
