using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ILocalUserFeedProcessor
    {
        Task Process();
    }
}