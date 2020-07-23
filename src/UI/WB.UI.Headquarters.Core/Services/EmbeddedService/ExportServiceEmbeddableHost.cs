using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;

namespace WB.UI.Headquarters.Services.EmbeddedService
{
    public class ExportServiceEmbeddableHost : BackgroundService
    {
        private readonly IOptions<HeadquartersConfig> headquarterOptions;
        private readonly ILogger<ExportServiceEmbeddableHost> logger;
        private readonly IConfiguration configuration;
        private readonly IServer server;

        public ExportServiceEmbeddableHost(
            IOptions<HeadquartersConfig> headquarterOptions,
            IConfiguration configuration,
            ILogger<ExportServiceEmbeddableHost> logger,
            IServer server)
        {
            this.headquarterOptions = headquarterOptions;
            this.logger = logger;
            this.server = server;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var exportHostPath = configuration.GetPathToExportServiceHostDll();

            if (exportHostPath == null)
            {
                logger.LogInformation("WB.Services.Export.Host.exe is not found in {search}");
                return;
            }

            var context = AssemblyLoadContext.Default;
            var resolver = new AssemblyDependencyResolver(exportHostPath);
            context.Resolving += (loadContext, name) =>
            {
                var assembly = resolver.ResolveAssemblyToPath(name);
                return assembly != null ? loadContext.LoadFromAssemblyPath(assembly) : null;
            };

            var exportHost = context.LoadFromAssemblyName(new AssemblyName("WB.Services.Export.Host"));

            var program = exportHost.GetType("WB.Services.Export.Host.Program");
            var createWebHostBuilder = program?.GetMethod("CreateWebHostBuilder");

            logger.LogInformation("Starting Export Service host");

            configuration["DataExport:ExportServiceUrl"] = "http://localhost:5555";

            IHostBuilder hostBuilder = createWebHostBuilder?.Invoke(null, new object[] { new string[] { } }) as IHostBuilder;

            if (hostBuilder == null)
            {
                logger.LogError("Unable to find IHostBuilder implementation at WB.Services.Export.Host.Program.CreateWebHostBuilder");
                return;
            }

            var serverUrl = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault(ip => ip.Contains("127.0.0.1"));

            if (serverUrl == null)
            {
                serverUrl = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault(ip => ip.Contains("localhost"));
            }

            hostBuilder.ConfigureWebHost(w =>
            {
                w.ConfigureAppConfiguration((ctx, c) =>
                {
                    c.Add(new MemoryConfigurationSource()
                    {
                        InitialData = new Dictionary<string, string>
                        {
                            ["TenantUrlOverride:" + this.headquarterOptions.Value.TenantName] = serverUrl
                        }
                    });
                });

                w.UseUrls("http://127.0.0.1:0"); 
            });

            var host = hostBuilder?.Build();
            
            var lifetime = host.Services.GetService<IHostApplicationLifetime>();

            lifetime.ApplicationStarted.Register(() =>
            {
                var exportServer = host.Services.GetService<IServer>();
                if (exportServer != null)
                {
                    var addresses = exportServer.Features.Get<IServerAddressesFeature>().Addresses;
                    configuration["DataExport:ExportServiceUrl"] = addresses.First();
                }
            });

            await host.RunAsync(stoppingToken);
        }
    }
}
