using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WB.Services.Infrastructure.Health;

namespace WB.Services.Scheduler.Storage
{
    internal class EfCoreHealthCheck : IHealthCheck
    {
        private readonly IOptions<JobSettings> jobSettings;
        private readonly DbContextOptions<JobContext> contextOptions;

        public EfCoreHealthCheck(IOptions<JobSettings> jobSettings, DbContextOptions<JobContext> contextOptions)
        {
            this.jobSettings = jobSettings;
            this.contextOptions = contextOptions;
        }

        public async Task<bool> CheckAsync()
        {
            using (var db = new JobContext(contextOptions, jobSettings))
            {
                await db.Jobs.FirstOrDefaultAsync();
                return true;
            }
        }

        public string Name => "EF Core migration status";
    }
}
