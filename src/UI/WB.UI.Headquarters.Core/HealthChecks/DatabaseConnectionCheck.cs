using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class DatabaseConnectionCheck : IHealthCheck
    {
        private readonly IInScopeExecutor scope;
        
        public DatabaseConnectionCheck(IInScopeExecutor scope)
        {
            this.scope = scope;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                await scope.ExecuteAsync(async sl =>
                {
                    var unitOfWork = sl.GetInstance<IUnitOfWork>();
                    await unitOfWork.Session.Connection.ExecuteAsync("select 1");
                });
            }
            catch(Exception e)
            {
                return HealthCheckResult.Unhealthy(Diagnostics.database_connection_check_Unhealty.FormatString(e), e);
            }

            return HealthCheckResult.Healthy(Diagnostics.database_connection_check_Healthy);
        }
    }
}
