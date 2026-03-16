using System;
using Microsoft.AspNetCore.Builder;

namespace WB.UI.Shared.Web.Integrity
{
    public static class IntegrityExtensions
    {
        public static IApplicationBuilder UseIntegrityHelper(this IApplicationBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            return builder.UseMiddleware<IntegrityHeaderMiddleware>();
        }
    }
}
