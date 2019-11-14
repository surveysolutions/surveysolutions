using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using StackExchange.Exceptional.Stores;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Logging;
using WB.UI.Shared.Web.Versions;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var autofacKernel = new AutofacKernel(builder);
            autofacKernel.Load(
                new NcqrsModule(),
                new NLogLoggingModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                //new CaptchaModule("recaptcha"),
                new WebInterviewModule(),
                new WebTesterModule(Configuration["DesignerAddress"]),
                new ProductVersionModule(typeof(Startup).Assembly, shouldStoreVersionToDb: false));
            this.Container = autofacKernel;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddControllersWithViews();
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddResponseCaching();
            services.AddResponseCompression();

            services.Configure<TesterConfiguration>(this.Configuration);
        }

        public AutofacKernel Container { get; set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions
            {
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
                routes.MapControllerRoute("Section", "WebTester/Interview/{id:Guid}/Section/{sectionId:Guid}", 
                    defaults: new { controller = "WebTester", action = "Section" });

                routes.MapControllerRoute("Interview", "WebTester/Interview/{id}/{*url}", new
                {
                    controller = "WebTester",
                    action = "Interview"
                });

                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=WebTester}/{action=Index}/{id?}");
            });
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(Container.Container));
            //var initTask = Container.InitAsync(false);
            //if (env.IsDevelopment())
            //    initTask.Wait();
            //else
            //    initTask.Wait(TimeSpan.FromSeconds(10));
        }
    }
}
