using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Scheduler.Services
{
  interface IHostedSchedulerService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
