using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using Prometheus.Advanced;
using WB.Services.Export.Checks;
using WB.Services.Export.Host.Infra;
using WB.Services.Export.Host.Jobs;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Utils;
using WB.Services.Scheduler;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Export.Host
{
    public class Startup
    {
        private readonly ILogger<Startup> logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            this.logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var webConfig = Configuration["webConfigs"];

            // should we go to web config for connection string?
            if (webConfig != null)
            {
                WebConfigReader.Read(Configuration, webConfig, logger);
            }

            services.AddMvcCore(ops =>
            {
                ops.ModelBinderProviders.Insert(0, new TenantEntityBinderProvider());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddJsonFormatters();

            services.AddTransient<IDataExportProcessesService, PostgresDataExportProcessesService>();

            services.Configure<DbConnectionSettings>(Configuration.GetSection("ConnectionStrings"));

            services.UseJobService(Configuration);
            services.RegisterJobHandler<ExportJobRunner>(ExportJobRunner.Name);
            services.AddScoped(typeof(ITenantApi<>), typeof(TenantApi<>));

            services.AddSingleton<ICollectorRegistry>(c =>
            {
                var registry = DefaultCollectorRegistry.Instance;
                var collectors = c.GetServices<IOnDemandCollector>();

                registry.RegisterOnDemandCollectors(collectors);
                registry.RegisterOnDemandCollector<DotNetStatsCollector>();
                
                return registry;
            });

            var healthChecksBuilder = services.AddHealthChecks();
            if (Configuration.IsS3Enabled())
            {
                healthChecksBuilder.AddCheck<AmazonS3Check>("Amazon s3");
            }

            healthChecksBuilder
                .AddCheck<EfCoreHealthCheck>("EF migrations")
                .AddDbContextCheck<JobContext>("Database");
            ServicesRegistry.Configure(services, Configuration);

            // Create the IServiceProvider based on the container.
            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseApplicationVersion("/.version");
            app.UseHealthChecks("/.hc");
            app.UseMetricServer("/metrics", app.ApplicationServices.GetService<ICollectorRegistry>());
            app.UseMvc();
        }
    }
}
