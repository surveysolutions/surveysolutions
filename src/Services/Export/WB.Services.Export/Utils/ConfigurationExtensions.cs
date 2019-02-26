using System;
using Microsoft.Extensions.Configuration;

namespace WB.Services.Export
{
    public static class ConfigurationExtensions
    {
        public static bool IsS3Enabled(this IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            return configuration.GetSection("Storage:S3").GetValue<bool>("Enabled");
        }
    }
}
