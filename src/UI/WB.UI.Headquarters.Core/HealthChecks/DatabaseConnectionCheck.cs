using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Resources;

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
                return HealthCheckResult.Unhealthy(Diagnostics.database_connection_check_Unhealty.FormatString(e), e);
            }

            return HealthCheckResult.Healthy(Diagnostics.database_connection_check_Healthy);
        }
    }
}
