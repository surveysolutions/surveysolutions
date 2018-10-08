using WB.Services.Export.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public interface ITenantApi<T>
    {
        T For(TenantInfo tenant);
    }
}
