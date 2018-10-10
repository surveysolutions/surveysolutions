using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using WB.Services.Export.Host.Infra;
using WB.Services.Export.Host.Jobs;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Infrastructure.Implementation;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Health;
using WB.Services.Scheduler;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(ops =>
                {
                    ops.ModelBinderProviders.Insert(0, new TenantEntityBinderProvider());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonFormatters();            

            services.AddTransient<IDataExportProcessesService, PostgresDataExportProcessesService>();
            
            services.UseJobService(Configuration);
            services.RegisterJobHandler<ExportJobRunner>(ExportJobRunner.Name);
            services.AddScoped(typeof(ITenantApi<>), typeof(TenantApi<>));

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
            app.UseMetricServer();
            app.UseMvc();
        }
    }
}
