using Microsoft.Extensions.Configuration;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquarterOptions
    {
        public string BaseUrl { get; set; }
        public string TenantName { get; set; }
        public object DataStorePath { get; set; } = "~/Add_Data";
    }

    public static class HeadquarterOptionsExtensions
    {
        public static IConfigurationSection HeadquarterOptions(this IConfiguration configuration) 
            => configuration.GetSection("Headquarters");
    }
}
