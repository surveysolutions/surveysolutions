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
            optionsBuilder.UseNpgsql(
                "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Qwerty1234;Database=ExportService_Factory;");

            var tenantInfo = new TenantInfo
            (
                baseUrl:"",
                id : TenantId.None,
                shortName : ""
            );
            var connectionSettings = Options.Create(new DbConnectionSettings
            {
                DefaultConnection = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Qwerty1234;Database=ExportService_Factory;"
            });
            var tenantContext = new TenantContext(null, tenantInfo);
            
            return new TenantDbContext(tenantContext, connectionSettings, optionsBuilder.Options);
        }
    }
}
