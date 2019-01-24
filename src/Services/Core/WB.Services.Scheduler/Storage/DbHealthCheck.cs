using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace WB.Services.Scheduler.Storage
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly IConfiguration configuration;

        public DbHealthCheck(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var connection = new NpgsqlConnection(this.configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.QueryAsync("SELECT version();");
                return HealthCheckResult.Healthy();
            }
        }
    }
}
