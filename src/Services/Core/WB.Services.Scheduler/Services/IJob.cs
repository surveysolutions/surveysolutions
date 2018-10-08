using System.Threading;
using System.Threading.Tasks;
using WB.Services.Scheduler.Services.Implementation;

namespace WB.Services.Scheduler.Services
{
    public interface IJob
    {
        Task ExecuteAsync(string arg, JobExecutingContext context, CancellationToken token);
    }
}
