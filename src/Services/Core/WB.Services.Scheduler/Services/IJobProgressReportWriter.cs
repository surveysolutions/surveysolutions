using System.Threading;
using System.Threading.Tasks;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services
{
    public interface IJobProgressReportWriter
    {
        Task WriteReportAsync(IJobEvent task, CancellationToken stoppingToken);
    }
}
