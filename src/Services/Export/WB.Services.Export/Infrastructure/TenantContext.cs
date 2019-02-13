using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public class TenantContext : ITenantContext, IDisposable
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IOptions<DbConnectionSettings> connectionSettings;
        private readonly DbContextOptions<TenantDbContext> options;

        public TenantContext(ITenantApi<IHeadquartersApi> tenantApi, 
            IOptions<DbConnectionSettings> connectionSettings,
            DbContextOptions<TenantDbContext> options)
        {
            this.tenantApi = tenantApi;
            this.connectionSettings = connectionSettings;
            this.options = options;
        }

        private TenantInfo tenant;

        public TenantInfo Tenant
        {
            get => tenant;
            set
            {
                tenant = value;
                Api = tenantApi?.For(value);
                this.DbContext = new TenantDbContext(this, connectionSettings, options);
            }
        }

        public IHeadquartersApi Api { get; private set; }

        public TenantDbContext DbContext { get; private set; }

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}
