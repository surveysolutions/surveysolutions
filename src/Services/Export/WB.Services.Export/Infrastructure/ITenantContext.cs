using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Infrastructure
{
    public interface ITenantContext
    {
        TenantInfo Tenant { get; set; }
        IHeadquartersApi Api { get; }
    }
}
