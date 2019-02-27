using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export
{
    public static class TenantInfoExtension
    {
        private static string DebugTag { get; set; } = string.Empty;

        public static void AddSchemaDebugTag(string tag)
        {
            DebugTag = tag;
        }

        public static string SchemaName(this TenantInfo tenantInfo)
        {
            return tenantInfo.Name + "_" + DebugTag + tenantInfo.Id;
        }
    }
}
