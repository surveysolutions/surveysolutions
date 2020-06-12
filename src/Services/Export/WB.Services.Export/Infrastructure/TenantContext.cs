﻿using System;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public class TenantContext : ITenantContext
    {
        private readonly ITenantApi<IHeadquartersApi>? tenantApi;

        public TenantContext(ITenantApi<IHeadquartersApi>? tenantApi, TenantInfo? tenant = null)
        {
            this.tenantApi = tenantApi;
            if(tenant!= null)
                Tenant = tenant;
        }

        private TenantInfo? tenant;
        public TenantInfo Tenant
        {
            get { return tenant ?? throw new InvalidOperationException("Tenant was not set.");}
            set { tenant = value; }
        }

        private IHeadquartersApi? api = null;
        public IHeadquartersApi Api
        {
            get { return api ??= (tenantApi?.For(Tenant) ?? throw new InvalidOperationException("TenantApi must be not null")); }
        }
    }
}
