using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Scheduler.Services
{
    internal interface IJobContextMigrator
    {
        Task MigrateAsync(CancellationToken token);
    }
}
