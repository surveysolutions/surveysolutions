using System;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface IInterviewsSynchronizer
    {
        void Pull();

        void Push(Guid userId);
    }
}