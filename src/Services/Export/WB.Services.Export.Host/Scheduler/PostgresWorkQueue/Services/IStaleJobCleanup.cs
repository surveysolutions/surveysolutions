using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services
{
    interface IStaleJobCleanup
    {
        Task ExecuteAsync(bool args, CancellationToken cancellationToken);
        Task ScheduleIfNeeded();
    }
}
