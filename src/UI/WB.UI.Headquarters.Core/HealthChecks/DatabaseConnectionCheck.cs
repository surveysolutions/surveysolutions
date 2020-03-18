using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.HealthChecks
{
    public class DatabaseConnectionCheck : IHealthCheck
    {
        private readonly IUnitOfWork unitOfWork;

        public DatabaseConnectionCheck(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                await unitOfWork.Session.Connection.ExecuteAsync("select 1");
            }
            catch(Exception e)
            {
                return HealthCheckResult.Unhealthy("Error during DB call", e);
            }

            return HealthCheckResult.Healthy("Connection to Database completed");
        }
    }
}
