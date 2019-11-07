using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Prometheus.Advanced;
using Serilog;
using WB.Services.Export.Checks;
using WB.Services.Export.Host.Infra;
using WB.Services.Export.Host.Jobs;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage.EfMappings;
using WB.Services.Export.Services.Processing;
using WB.Services.Scheduler;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Export.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var webConfig = Configuration["webConfigs"];

            // should we go to web config for connection string?
            if (webConfig != null)
            {
                WebConfigReader.Read(Configuration, webConfig);
            }

            services.AddTransient<TenantModelBinder>();

            services.AddControllers(opts =>
            {
                opts.ModelBinderProviders.Insert(0, new TenantEntityBinderProvider());
                opts.Filters.Add<TenantInfoPropagationActionFilter>();
            }).AddNewtonsoftJson();

            services.AddTransient<IDataExportProcessesService, PostgresDataExportProcessesService>();

            services.Configure<DbConnectionSettings>(Configuration.GetSection("ConnectionStrings"));

            services.UseJobService(Configuration);

            services.RegisterJobHandler<ExportJobRunner>(ExportJobRunner.Name);
            services.AddScoped(typeof(ITenantApi<>), typeof(TenantApi<>));
            services.AddDbContext<TenantDbContext>(builder =>
            {
                builder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
            });
            services.AddTransient<IOnDemandCollector, AppVersionCollector>();

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

            services.Configure(Configuration);

#if RANDOMSCHEMA && DEBUG
            TenantInfoExtension.AddSchemaDebugTag(Process.GetCurrentProcess().Id.ToString() + "_");
#endif
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Export service started. version {version}",
                FileVersionInfo.GetVersionInfo(this.GetType().Assembly.Location).ProductVersion);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.StartScheduler();
            app.UseHealthChecks("/.hc");

            app.UseRouting();

            app.UseEndpoints(e =>
            {
                e.MapControllers();
            });

            Log.Logger.Information("Export service started. version {version}");
            //app.UseMetricServer("/metrics", app.ApplicationServices.GetService<ICollectorRegistry>());

        }
    }
}
