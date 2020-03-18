using System.Data.Common;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    public interface ITenantContext
    {
        TenantInfo Tenant { get; }
        IHeadquartersApi Api { get; }
        void SetTenant(TenantInfo tenant);
    }
}
