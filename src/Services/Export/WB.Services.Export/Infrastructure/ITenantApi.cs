using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Infrastructure
{
    public interface ITenantApi<T>
    {
        IHeadquartersApi For(TenantInfo? tenant);
    }
}
