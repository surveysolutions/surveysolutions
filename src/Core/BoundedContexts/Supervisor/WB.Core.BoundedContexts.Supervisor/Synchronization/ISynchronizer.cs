using System;
using System.Threading.Tasks;
using Quartz;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ISynchronizer
    {
        void Pull();
        void Push(Guid userId);
    }
}