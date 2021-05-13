using Microsoft.Extensions.Configuration;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersConfig
    {
        public string BaseUrl { get; set; }
        public string BaseAppUrl { get; set; }
        public string TenantName { get; set; }
        public bool IgnoreCompatibility { get; set; } = false;
    }

    public static class HeadquarterOptionsExtensions
    {
        public static IConfigurationSection HeadquarterOptions(this IConfiguration configuration) 
            => configuration.GetSection("Headquarters");
    }
}
