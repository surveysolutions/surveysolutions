using System;
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
            var nameLength = tenantInfo.ShortName.Length;

            if (tenantInfo.Workspace == TenantInfo.DefaultWorkspace)
            {
                return tenantInfo.ShortName.Substring(0, Math.Min(nameLength, 8)) + "_" + DebugTag + tenantInfo.Id;
            }

            return tenantInfo.ShortName.Substring(0, Math.Min(nameLength, 8))
                   + "_"
                   + tenantInfo.Workspace.Substring(0, Math.Min(tenantInfo.Workspace.Length, 8)) 
                   + "_" 
                   + DebugTag 
                   + tenantInfo.Id;
        }
    }
}
