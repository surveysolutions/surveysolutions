using System.Diagnostics;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export
{
#if RANDOMSCHEMA
    public static class TenantInfoExtension
    {
        private static readonly long pid;

        static TenantInfoExtension()
        {
            pid = Process.GetCurrentProcess().Id;
        }

        public static string SchemaName(this TenantInfo tenantInfo) =>
            tenantInfo.Name + "_" + pid + "_" + tenantInfo.Id;
    }
#else
    public static class TenantInfoExtension
    {
        public static string SchemaName(this TenantInfo tenantInfo) =>
            tenantInfo.Name + "_" + tenantInfo.Id;
    }
#endif
}
