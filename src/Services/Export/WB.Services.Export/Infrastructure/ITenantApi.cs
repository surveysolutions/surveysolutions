using Autofac;
using WB.Services.Export.Infrastructure.Implementation;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public interface ITenantApi<T>
    {
        T For(TenantInfo tenant);
    }
}
