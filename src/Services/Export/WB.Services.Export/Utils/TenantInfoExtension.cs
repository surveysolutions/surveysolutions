using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export
{
    public static class TenantInfoExtension
    {
        public static string SchemaName(this TenantInfo tenantInfo) => tenantInfo.Name + "_" + tenantInfo.Id;
    }
}
