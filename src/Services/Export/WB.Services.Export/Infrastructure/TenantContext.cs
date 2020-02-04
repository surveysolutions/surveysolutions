using System;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public class TenantContext : ITenantContext
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;

        public TenantContext(ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.tenantApi = tenantApi;
        }

        private TenantInfo tenant;

        public TenantInfo Tenant
        {
            get => tenant;
            set
            {
                tenant = value;
                Api = tenantApi?.For(value);
            }
        }

        public IHeadquartersApi Api { get; private set; }
        public void SetTenant(TenantInfo tenant)
        {
            this.Tenant = tenant;
        }
    }
}
