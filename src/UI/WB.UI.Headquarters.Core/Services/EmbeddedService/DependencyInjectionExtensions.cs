using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.UI.Headquarters.HealthChecks;

namespace WB.UI.Headquarters.Services.EmbeddedService
{
    public static class DependencyInjectionExtensions
    {
        public static ExportMode GetExportMode(this IConfiguration configuration)
        {
            var modeString = configuration["DataExport:ExportMode"];
            if (!string.IsNullOrEmpty(modeString) && Enum.TryParse<ExportMode>(modeString, out var exportMode))
            {
                return exportMode;
            }

            return ExportMode.Auto;
        }
        
        public static int GetExportPort(this IConfiguration configuration)
        {
            var portString = configuration["DataExport:ExportPort"];
            if (!string.IsNullOrEmpty(portString) && int.TryParse(portString, out var exportPort))
            {
                return exportPort;
            }

            return 0;
        }
        
        public static string GetPathToExportServiceHostDll(this IConfiguration configuration)
        {
            var exportFolder = configuration["DataExport:EmbeddedExportSearchPath"] ?? "./Export.Service";
            
            var path = Path.GetFullPath(exportFolder);
            var exportHostPath = Path.Combine(path, "WB.Services.Export.Host.dll");

            Serilog.Log.Verbose("Looking for WB.Services.Export.Host.dll at {directory}", path);
            if (!System.IO.File.Exists(exportHostPath))
            {
                // looking for export relative to assembly location
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = new FileInfo(assemblyLocation).Directory;

                if (assemblyDirectory != null)
                {
                    Serilog.Log.Verbose("Looking WB.Services.Export.Host.dll at {directory}", assemblyDirectory.FullName);
                    var export = Path.Combine(assemblyDirectory.FullName, exportFolder, "WB.Services.Export.Host.dll");

                    if (System.IO.File.Exists(export))
                    {
                        return export;
                    }
                }
            }

            return exportHostPath;
        }

        public static IHostBuilder ConfigureEmbeddedServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    var exportMode = context.Configuration.GetExportMode();
                    if (exportMode == ExportMode.Client)
                        return;
                    
                    var exportServiceHostDll = context.Configuration.GetPathToExportServiceHostDll();
                    if (System.IO.File.Exists(exportServiceHostDll))
                    {
                        services.AddHostedService<ExportServiceEmbeddableHost>();

                        if (!context.IsRunningOnIIS())
                        {
                            var u = context.Configuration[WebHostDefaults.ServerUrlsKey];
                            context.Configuration[WebHostDefaults.ServerUrlsKey] = u + ";http://127.0.0.1:0";
                        }

                        services.AddSingleton<EmbeddedExportServiceHealthCheck>();
                        services.AddHealthChecks().AddCheck<EmbeddedExportServiceHealthCheck>("embedded_export_service_check");
                    }
                    else
                    {
                        Serilog.Log.Logger.Information("No Embedded Export Service Host configured");
                        Serilog.Log.Logger.Verbose("SearchPath: {searchPath}", exportServiceHostDll);
                    }
                });
        }

        public static bool IsRunningOnIIS(this HostBuilderContext ctx)
        {
            return Environment.GetEnvironmentVariable("APP_POOL_ID") != null;
        }
    }
}
