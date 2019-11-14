using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using StackExchange.Exceptional.Stores;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Logging;
using WB.UI.Shared.Web.Versions;
using WB.UI.WebTester;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace WebTester
{
    public class Startup
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        private AspCoreKernel aspCoreKernel;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            EnsureJsonStorageForErrorsExists();
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"Application started {FileVersionInfo.GetVersionInfo(typeof(Startup).Assembly.Location).ProductVersion}");

            aspCoreKernel = new AspCoreKernel(services);

            aspCoreKernel.Load(
                new NcqrsModule(),
                new NLogLoggingModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                //new CaptchaModule("recaptcha"),
                new WebInterviewModule(),
                new WebTesterModule(this.hostingEnvironment),
                new ProductVersionModule(typeof(Startup).Assembly, shouldStoreVersionToDb: false)
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Questionnaire}/{action=My}/{id?}"
                );

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Questionnaire}/{action=Index}/{id?}");
            });

            var initTask = aspCoreKernel.InitAsync(serviceProvider);
            if (env.IsDevelopment())
                initTask.Wait();
            else
                initTask.Wait(TimeSpan.FromSeconds(10));
        }

        private void EnsureJsonStorageForErrorsExists()
        {
            if (StackExchange.Exceptional.Exceptional.Settings.DefaultStore is JSONErrorStore exceptionalConfig)
            {
                var jsonStorePath = exceptionalConfig.Settings.Path;
                var jsonStorePathAbsolute = hostingEnvironment.ContentRootFileProvider.GetFileInfo(jsonStorePath).PhysicalPath;

                if (!Directory.Exists(jsonStorePathAbsolute))
                {
                    Directory.CreateDirectory(jsonStorePathAbsolute);
                }
            }
        }
    }
}
