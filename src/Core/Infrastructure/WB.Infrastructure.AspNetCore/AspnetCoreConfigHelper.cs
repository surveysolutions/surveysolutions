using Microsoft.Extensions.Configuration;

namespace WB.Infrastructure.AspNetCore
{
    public static class CoreConfigHelper
    {
        public static LoggingConfig LoggingConfig(this IConfiguration configuration)
        {
            return configuration.GetSection("Logging").Get<LoggingConfig>() 
                ?? new LoggingConfig();
        }
    }
}
