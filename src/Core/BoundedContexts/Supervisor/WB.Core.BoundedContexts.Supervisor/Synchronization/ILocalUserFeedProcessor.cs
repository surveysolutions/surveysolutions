using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ILocalUserFeedProcessor
    {
        Guid[] PullUsersAndReturnListOfSynchronizedSupervisorsId();
    }
}