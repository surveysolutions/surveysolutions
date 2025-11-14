using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    /// <summary>
    /// This class only needed for local development. It's not used in runtime.
    /// This class required for `Add-Migration` to work from Package Manager Console
    /// </summary>
    public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql("...");

            var tenantInfo = new TenantInfo
            (
                baseUrl:"",
                id : TenantId.None,
                shortName : ""
            );
            var connectionSettings = Options.Create(new DbConnectionSettings
            {
                DefaultConnection = "..."
            });
            var tenantContext = new TenantContext(null, tenantInfo);
            
            return new TenantDbContext(tenantContext, connectionSettings, optionsBuilder.Options);
        }
    }
}
