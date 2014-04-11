using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public interface ISynchronizer
    {
        Task FillLocalCopyOfFeed();
    }
}