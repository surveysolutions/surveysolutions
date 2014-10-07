using System;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface IInterviewsSynchronizer
    {
        void PullInterviewsForSupervisors(Guid[] supervisorIds);

        void Push(Guid userId);
    }
}