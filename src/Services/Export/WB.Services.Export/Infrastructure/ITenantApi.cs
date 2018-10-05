using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public interface ITenantApi<T>
    {
        T For(TenantInfo tenant);
    }
}
