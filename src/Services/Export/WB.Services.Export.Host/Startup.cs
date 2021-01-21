using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

using Serilog;
using WB.Services.Export.Checks;
using WB.Services.Export.Host.Infra;
using WB.Services.Export.Host.Jobs;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage.EfMappings;
using WB.Services.Export.Services.Processing;
using WB.Services.Scheduler;
using WB.Services.Scheduler.Stats;
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

            var healthChecksBuilder = services.AddHealthChecks();
            if (Configuration.IsS3Enabled())
            {
                healthChecksBuilder.AddCheck<AmazonS3Check>("Amazon s3");
            }

            healthChecksBuilder
                .AddCheck<EfCoreHealthCheck>("EF migrations");

            services.Configure(Configuration);

#if RANDOMSCHEMA && DEBUG
            TenantInfoExtension.AddSchemaDebugTag(Process.GetCurrentProcess().Id.ToString() + "_");
#endif
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once UnusedMember.Global Used by Aspnet core
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Export service starting. version {version}",
                FileVersionInfo.GetVersionInfo(this.GetType().Assembly.Location).ProductVersion);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();

            app.UseSchedulerMetrics();            

            app.UseRouting();

            app.UseEndpoints(e =>
            {
                e.MapHealthChecks("/.hc");
                e.MapControllers();
            });

            Log.Logger.Information("Export service started.");

        }
    }
}
