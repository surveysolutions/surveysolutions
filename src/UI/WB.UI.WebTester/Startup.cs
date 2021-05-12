using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Refit;
using Serilog;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.AspNetCore;
using WB.Infrastructure.Native.Storage;
using WB.UI.Shared.Web.Controllers;
using WB.UI.Shared.Web.Diagnostics;
using WB.UI.Shared.Web.LoggingIntegration;
using WB.UI.Shared.Web.Versions;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester
{
    public class Startup
    {
        private AutofacKernel? autofacKernel;

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
            services.AddSignalR()
                .AddNewtonsoftJsonProtocol();
            services.Configure<TesterConfiguration>(this.Configuration);
            services.AddHttpContextAccessor();

            services.AddHealthChecks()
                .AddCheck<DesignerConnectionCheck>("designer-connection");

            services.AddHttpClientWithConfigurator<IDesignerWebTesterApi, DesignerApiConfigurator>(
                    new RefitSettings
                    {
                        ContentSerializer = new NewtonsoftJsonContentSerializer(new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() },
                            SerializationBinder = new OldToNewAssemblyRedirectSerializationBinder()
                        })
                    })
#if DEBUG
           .ConfigurePrimaryHttpMessageHandler(() =>
               new HttpClientHandler
               {
                   ClientCertificateOptions = ClientCertificateOption.Manual,
                   ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
               })
#endif
           ;
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
                new SerilogLoggerModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                //new CaptchaModule("recaptcha"),
                new WebInterviewModule(Configuration), // init registers denormalizer
                new WebTesterModule(),
                new ProductVersionModule(typeof(Startup).Assembly, shouldStoreVersionToDb: false)); // stores app version in database but does not do it for web tester
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(autofacKernel == null)
                throw new InvalidOperationException("Kernel must not be null.");

            var initTask = autofacKernel.InitAsync(true);
            initTask.Wait(TimeSpan.FromSeconds(5));

            if (!env.IsDevelopment())
            {
                app.UseStatusCodePagesWithReExecute("/error/{0}");
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
            app.UseSerilogRequestLogging(o => o.Logger = app.ApplicationServices.GetService<ILogger>());
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
                    new CultureInfo("zh"),
                    new CultureInfo("ro")
                };
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapVersionEndpoint();
                endpoints.MapHealthChecks(".hc");

                endpoints.MapDefaultControllerRoute();

                endpoints.MapHub<WebInterview>("interview",
                    options => { options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling; });
            });
          
        }
    }
}
