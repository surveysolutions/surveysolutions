using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WB.UI.Headquarters.Services.EmbeddedService
{
    public static class DependencyInjectionExtensions
    {
        public static string GetPathToExportServiceHostDll(this IConfiguration configuration)
        {
            var exportFolder = configuration["DataExport:EmbeddedExportSearchPath"];

            var path = Path.GetFullPath(exportFolder);
            var exportHostPath = Path.Combine(path, "WB.Services.Export.Host.dll");

            if (!System.IO.File.Exists(exportHostPath))
            {
                return null;
            }

            return exportHostPath;
        }

        public static IHostBuilder ConfigureEmbeddedServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    if (context.Configuration.GetPathToExportServiceHostDll() != null)
                    {
                        services.AddHostedService<ExportServiceEmbeddableHost>();
                    }
                })

                .ConfigureAppConfiguration(c =>
                {
                    c.Add(new MemoryConfigurationSource());
                })

                .ConfigureWebHost(w =>
                {
                    var u = w.GetSetting(WebHostDefaults.ServerUrlsKey);
                    w.UseSetting(WebHostDefaults.ServerUrlsKey, u + ";http://127.0.0.1:0");
                });
        }
    }
}
