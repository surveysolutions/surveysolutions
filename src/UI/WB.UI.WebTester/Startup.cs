using System;
using System.Collections.Generic;
using System.Globalization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Logging;
using WB.UI.Shared.Web.Controllers;
using WB.UI.Shared.Web.Versions;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester
{
    public class Startup
    {
        private AutofacKernel autofacKernel;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddControllersWithViews()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.Converters.Add(new EnumToStringConverter());
                });
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddResponseCaching();
            services.AddResponseCompression();
            services.AddLogging();
            services.AddSignalR()
                .AddNewtonsoftJsonProtocol();
            services.Configure<TesterConfiguration>(this.Configuration);
            services.AddHttpContextAccessor();

            services.AddHealthChecks()
                .AddCheck<DesignerConnectionCheck>("designer-connection");

        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            autofacKernel = new AutofacKernel(builder);
            autofacKernel.Load(
                new NcqrsModule(),
                new NLogLoggingModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                //new CaptchaModule("recaptcha"),
                new WebInterviewModule(), // init registers denormalizer
                new WebTesterModule(Configuration["DesignerAddress"]),
                new ProductVersionModule(typeof(Startup).Assembly, shouldStoreVersionToDb: false)); // stores app version in database but does not do it for web tester
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var initTask = autofacKernel.InitAsync(true);

            if (!env.IsDevelopment())
            {
                app.UseStatusCodePagesWithReExecute("/error/{0}");
                app.UseHttpsRedirection();
                initTask.Wait();

            }
            else
            {
                initTask.Wait(TimeSpan.FromSeconds(10));
            }
            
            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/Content",
                OnPrepareResponse = ctx =>
                {
                    if (!env.IsDevelopment())
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
                    }
                }
            });

            app.UseCookiePolicy();
            app.UseSession();

            app.UseRequestLocalization(opt =>
            {
                opt.DefaultRequestCulture = new RequestCulture("en-US");
                opt.SupportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en")
                };
                opt.SupportedUICultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru"),
                    new CultureInfo("fr"),
                    new CultureInfo("es"),
                    new CultureInfo("ar"),
                    new CultureInfo("zh")
                };
            });

            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                routes.MapHealthChecks(".hc");

                routes.MapControllerRoute("Section", "WebTester/Interview/{id:Guid}/Section/{sectionId:Guid}", 
                    defaults: new { controller = "WebTester", action = "Section" });

                routes.MapControllerRoute("Interview", "WebTester/Interview/{id}/{*url}", new
                {
                    controller = "WebTester",
                    action = "Interview"
                });

                routes.MapHub<WebInterview>("interview",
                    options => { options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling; });

                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=WebTester}/{action=Index}/{id?}");
            });
          
        }
    }
}
