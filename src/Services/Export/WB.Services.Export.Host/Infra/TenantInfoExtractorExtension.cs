using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Host.Infra
{
    public static class TenantInfoExtractorExtension
    {
        public static TenantInfo? GetTenantInfo(this HttpRequest request)
        {
            var headers = request.Headers;
            var auth = headers["Authorization"];

            if (!auth.Any() || !auth.ToString().StartsWith("Bearer "))
            {
                return null;
            }

            string baseUrl = headers["Referer"];
            string tenantId = headers["Authorization"].ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            string name = headers["x-tenant-name"];
            string space = headers["x-tenant-space"];

            return new TenantInfo(baseUrl, tenantId, name, space);
        }
    }
}
