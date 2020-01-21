using Microsoft.AspNetCore.Builder;

namespace WB.UI.Shared.Web.UnderConstruction
{
    public static class UnderConstructionExtensions
    {
        public static IApplicationBuilder UseUnderConstruction(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnderConstructionMiddleware>();
        }
    }
}
