using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

            return exportHostPath;
        }

        public static IHostBuilder ConfigureEmbeddedServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    var exportServiceHostDll = context.Configuration.GetPathToExportServiceHostDll();
                    if (System.IO.File.Exists(exportServiceHostDll))
                    {
                        services.AddHostedService<ExportServiceEmbeddableHost>();
                        var u = context.Configuration[WebHostDefaults.ServerUrlsKey];
                        context.Configuration[WebHostDefaults.ServerUrlsKey] = u + ";http://127.0.0.1:0";
                    }
                    else
                    {
                        Serilog.Log.Logger.Information("No Embedded Export Service Host configured");
                        Serilog.Log.Logger.Verbose("SearchPath: {searchPath}", exportServiceHostDll);
                    }
                });
        }
    }
}
