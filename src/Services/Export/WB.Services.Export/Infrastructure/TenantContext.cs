using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public class TenantContext : ITenantContext
    {
        public TenantInfo Tenant { get; set; }
    }
}