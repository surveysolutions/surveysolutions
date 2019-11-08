using System.Web.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WB.UI.WebTester
{
    public static class ConfigurationSource
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public static void Init(IHostingEnvironment hostingEnvironment)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(hostingEnvironment.WebRootFileProvider.GetFileInfo("~/Configuration").PhysicalPath);
            builder.AddJsonFile("config.json", false, true);
            builder.AddJsonFile("config.override.json", true, true);
            builder.AddJsonFile($"config.{System.Environment.MachineName}.json", true, true);

            Configuration = builder.Build();
        }
    }
}
