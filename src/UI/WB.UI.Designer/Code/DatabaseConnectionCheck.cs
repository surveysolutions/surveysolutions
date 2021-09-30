using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.UI.Designer.Code
{
    public class DatabaseConnectionCheck : IHealthCheck
    {
        private readonly DesignerDbContext dbContext;

        public DatabaseConnectionCheck(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                await this.dbContext.ProductVersionChanges.FirstOrDefaultAsync(cancellationToken);
            }
            catch
            {
                return HealthCheckResult.Unhealthy();
            }            
            
            return HealthCheckResult.Healthy();
        }
    }
}
