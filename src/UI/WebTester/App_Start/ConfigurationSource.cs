using System.Web.Hosting;
using Microsoft.Extensions.Configuration;

namespace WB.UI.WebTester
{
    public static class ConfigurationSource
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public static void Init()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(HostingEnvironment.MapPath("~/Configuration"));
            builder.AddJsonFile("config.json", false, true);
            builder.AddJsonFile("config.override.json", true, true);
            builder.AddJsonFile($"config.{System.Environment.MachineName}.json", true, true);

            Configuration = builder.Build();
        }
    }
}