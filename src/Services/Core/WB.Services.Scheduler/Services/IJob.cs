using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Scheduler.Services
{
    public interface IJob
    {
        Task ExecuteAsync(string arg, CancellationToken token);
    }
}
