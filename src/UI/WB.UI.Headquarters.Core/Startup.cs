using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Anemonis.AspNetCore.RequestDecompression;
using Autofac;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Persistence.Headquarters.Migrations.Events;
using WB.Persistence.Headquarters.Migrations.Logs;
using WB.Persistence.Headquarters.Migrations.PlainStore;
using WB.Persistence.Headquarters.Migrations.Quartz;
using WB.Persistence.Headquarters.Migrations.ReadSide;
using WB.Persistence.Headquarters.Migrations.Users;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.Authentication;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.HealthChecks;
using WB.UI.Headquarters.Metrics;
using WB.UI.Headquarters.Models.Api.DataTable;
using WB.UI.Headquarters.Models.Users;
using WB.UI.Shared.Web.Diagnostics;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.LoggingIntegration;
using WB.UI.Shared.Web.UnderConstruction;
using WB.UI.Shared.Web.Versions;

namespace WB.UI.Headquarters
{
    public class Startup
    {
        private readonly IWebHostEnvironment environment;
        private AutofacKernel autofacKernel;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            autofacKernel = new AutofacKernel(builder, c => InScopeExecutor.Init(new UnitOfWorkInScopeExecutor(c)));

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var unitOfWorkConnectionSettings = BuildUnitOfWorkSettings(connectionString);

            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                .Where(x => x?.Namespace?.Contains("Services.Impl") == true)
                .AsImplementedInterfaces();

            var eventStoreSettings = new PostgreConnectionSettings
            {
                ConnectionString = connectionString,
                SchemaName = "events"
            };

            var eventStoreModule = new PostgresWriteSideModule(eventStoreSettings,
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly, typeof(M001_AddEventSequenceIndex).Namespace));

            autofacKernel.Load(
                new NcqrsModule(),
                new SerilogLoggerModule(),
                eventStoreModule,
                new InfrastructureModule(),
                new DataCollectionSharedKernelModule(),
                new WebInterviewModule(),
                new DataCollectionSharedKernelModule(),
                new OrmModule(unitOfWorkConnectionSettings),
                new OwinSecurityModule(),
                new FileStorageModule(),
                new FileInfrastructureModule(),
                new DataExportModule(),
                GetHqBoundedContextModule(),
                new HeadquartersUiModule(Configuration),
                new ProductVersionModule(typeof(Startup).Assembly),
                GetQuartzModule()
                );
        }

        public static UnitOfWorkConnectionSettings BuildUnitOfWorkSettings(string connectionString)
        {
            var mappingAssemblies = new List<Assembly> { typeof(HeadquartersBoundedContextModule).Assembly };

            var unitOfWorkConnectionSettings = new UnitOfWorkConnectionSettings
            {
                ConnectionString = connectionString,
                ReadSideMappingAssemblies = mappingAssemblies,
                PlainStorageSchemaName = "plainstore",
                PlainMappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                },
                PlainStoreUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace),
                ReadSideUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_InitDb).Namespace),
                LogsUpgradeSettings = new DbUpgradeSettings(typeof(M201905171139_AddErrorsTable).Assembly,
                    typeof(M201905171139_AddErrorsTable).Namespace),
                UsersUpgradeSettings = DbUpgradeSettings.FromFirstMigration<M001_AddUsersHqIdentityModel>()
            };
            return unitOfWorkConnectionSettings;
        }

        private QuartzModule GetQuartzModule()
        {
            var schedulerSection = this.Configuration.GetSection("Scheduler");

            return new QuartzModule(typeof(M201905151013_AddQuartzTables).Assembly,
                typeof(M201905151013_AddQuartzTables).Namespace,
                schedulerSection["InstanceId"],
                schedulerSection["IsClustered"].ToBool(false));
        }

        private HeadquartersBoundedContextModule GetHqBoundedContextModule()
        {
            var configurationSection = Configuration.GetSection("PreLoading").Get<PreloadingConfig>();
            var sampleImportSettings = new SampleImportSettings(
                 configurationSection.InterviewsImportParallelTasksLimit);

            var trackingSection = Configuration.GetSection("Tracking").Get<TrackingConfig>();

            var syncPackageSection =
                Configuration.GetSection("SyncPackageReprocessor").Get<SyncPackageReprocessorConfig>();

            var userPreloadingSettings =
                new UserPreloadingSettings(
                    configurationSection.MaxAllowedRecordNumber,
                    loginFormatRegex: CreateUserModel.UserNameRegularExpression,
                    emailFormatRegex: configurationSection.EmailFormatRegex,
                    phoneNumberFormatRegex: configurationSection.PhoneNumberFormatRegex,
                    fullNameMaxLength: EditUserModel.PersonNameMaxLength,
                    phoneNumberMaxLength: EditUserModel.PhoneNumberLength,
                    personNameFormatRegex: EditUserModel.PersonNameRegex);

            var synchronizationSettings = new SyncSettings(origin: Constants.SupervisorSynchronizationOrigin);

            ExternalStoragesSettings externalStoragesSettings = new FakeExternalStoragesSettings();

            if (Configuration.GetSection("ExternalStorages").Exists())
            {
                externalStoragesSettings = Configuration.GetSection("ExternalStorages").Get<ExternalStoragesSettings>();
            }

            return new HeadquartersBoundedContextModule(userPreloadingSettings,
                sampleImportSettings,
                synchronizationSettings,
                new TrackingSettings(trackingSection.WebInterviewPauseResumeGraceTimespan),
                externalStoragesSettings: externalStoragesSettings,
                fileSystemEmailServiceSettings:
                    new FileSystemEmailServiceSettings(false, null, null, null, null, null),
                syncPackagesJobSetting: new SyncPackagesProcessorBackgroundJobSetting(true, syncPackageSection.SynchronizationInterval, syncPackageSection.SynchronizationBatchCount, syncPackageSection.SynchronizationParallelExecutorsCount)
            );
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUnderConstruction();

            services.AddOptions();
            services.AddControllersWithViews().AddNewtonsoftJson(j =>
            {
                //j.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                j.SerializerSettings.Converters.Add(new StringEnumConverter());
                j.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddResponseCaching();
            services.AddResponseCompression();
            services.AddSignalR().AddNewtonsoftJsonProtocol();

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(Startup));

            services.AddRazorPages();
            services.AddHttpContextAccessor();

            services.AddHqAuthorization();
            services.AddDatabaseStoredExceptional(environment, Configuration);

            services.AddScoped<UnitOfWorkActionFilter>();
            services.AddScoped<InstallationFilter>();
            services.AddScoped<AntiForgeryFilter>();
            services.AddScoped<GlobalNotificationResultFilter>();
            services.AddTransient<ObserverNotAllowedActionFilter>();
            services.AddHeadquartersHealthCheck();

            FileStorageModule.Setup(services, Configuration);

            AddCompression(services);

            services.AddMvc(mvc =>
            {
                mvc.Filters.AddService<UnitOfWorkActionFilter>(1);
                mvc.Filters.AddService<InstallationFilter>(100);
                mvc.Filters.AddService<GlobalNotificationResultFilter>(200);
                mvc.Filters.AddService<ObserverNotAllowedActionFilter>(300);
                mvc.Conventions.Add(new OnlyPublicApiConvention());
                mvc.ModelBinderProviders.Insert(0, new DataTablesRequestModelBinderProvider());
                var noContentFormatter = mvc.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
                if (noContentFormatter != null)
                {
                    noContentFormatter.TreatNullValueAsNoContent = false;
                }
            })
#if DEBUG
                .AddRazorRuntimeCompilation()
#endif
                ;

            services.AddHqSwaggerGen();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // https://github.com/JanKallman/EPPlus/issues/31

            services.AddOptionsConfiguration(this.Configuration);

#if RELEASE            
            var physicalProvider =  environment.ContentRootFileProvider;
            var manifestEmbeddedProvider = new Microsoft.Extensions.FileProviders.ManifestEmbeddedFileProvider(typeof(Program).Assembly, "wwwroot");
            var compositeProvider = new Microsoft.Extensions.FileProviders.CompositeFileProvider(physicalProvider, manifestEmbeddedProvider);

            services.AddSingleton<Microsoft.Extensions.FileProviders.IFileProvider>(compositeProvider);
            environment.WebRootFileProvider = compositeProvider;
#endif
            if (Configuration["no-quartz"].ToBool(false) == false)
            {
                services.AddHostedService<QuartzHostedService>();
            }

            var passwordOptions = Configuration.GetSection("PasswordOptions").Get<PasswordOptions>();

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = passwordOptions.RequireDigit;
                options.Password.RequireLowercase = passwordOptions.RequireLowercase;
                options.Password.RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric;
                options.Password.RequireUppercase = passwordOptions.RequireUppercase;
                options.Password.RequiredLength = passwordOptions.RequiredLength;
                options.Password.RequiredUniqueChars = passwordOptions.RequiredUniqueChars;
            });
        }

        private static void AddCompression(IServiceCollection services)
        {
            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });

            services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddRequestDecompression(o => { o.Providers.Add<GzipDecompressionProvider>(); });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });

            services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
            services.AddMetrics();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptional();

            if (!env.IsDevelopment())
            {
                app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
                    appBuilder =>
                    {
                        appBuilder.UseStatusCodePagesWithReExecute("/error/{0}");
                    });

                app.UseHsts();
            }

            app.UseMiddleware<ExceptionHandler>();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (!env.IsDevelopment())
                    {
                        ctx.Context.Response.Headers.Add("Cache-Control", "public, max-age=31536000");
                    }
                }
            });

            // make sure we do not track static files requests
            app.UseMetrics(Configuration);

            app.UseSerilogRequestLogging();
            app.UseUnderConstruction();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSession();
            app.UseResponseCompression();
            app.UseRequestDecompression();

            app.UseHqSwaggerUI();

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
                    new CultureInfo("id"),
                    new CultureInfo("pt"),
                    new CultureInfo("zh")
                };
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapVersionEndpoint();

                endpoints.MapHealthChecks(".hc", new HealthCheckOptions { AllowCachingResponses = false });

                // split readiness probe from live health checks
                // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1#separate-readiness-and-liveness-probes
                endpoints.MapHealthChecks(".hc/ready", new HealthCheckOptions
                {
                    AllowCachingResponses = false, 
                    ResultStatusCodes =
                    {
                        // return Server Unavailable on Degraded status
                        [HealthStatus.Degraded] = 503
                    },
                    Predicate = c => c.Tags.Contains("ready")
                });

                endpoints.MapDefaultControllerRoute();

                endpoints.MapHub<WebInterview>("interview");

                // obsolete since all interviewers will be 20.03 and higher
                endpoints.MapGet("/Dependencies/img/logo.png", async context =>
                {
                    await context.Response.WriteAsync("Ok");
                });
            });

            InitModules(env);
        }

        private void InitModules(IWebHostEnvironment env)
        {
            var initTask = autofacKernel.InitAsync(true);
            initTask.Wait(TimeSpan.FromSeconds(5));
        }
    }
}
