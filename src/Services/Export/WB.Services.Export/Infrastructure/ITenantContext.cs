using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public interface ITenantContext
    {
        TenantInfo Tenant { get; set; }
    }
}