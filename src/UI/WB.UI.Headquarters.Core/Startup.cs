using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Anemonis.AspNetCore.RequestDecompression;
using Autofac;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Refit;
using Serilog;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.AspNetCore.DataProtection;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Persistence.Headquarters.Migrations.Logs;
using WB.Persistence.Headquarters.Migrations.MigrateToPrimaryWorkspace;
using WB.Persistence.Headquarters.Migrations.PlainStore;
using WB.Persistence.Headquarters.Migrations.Quartz;
using WB.Persistence.Headquarters.Migrations.ReadSide;
using WB.Persistence.Headquarters.Migrations.Users;
using WB.Persistence.Headquarters.Migrations.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.Authentication;
using WB.UI.Headquarters.Code.ResetPassword;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.HealthChecks;
using WB.UI.Headquarters.Metrics;
using WB.UI.Headquarters.Models.Api.DataTable;
using WB.UI.Headquarters.Models.Users;
using WB.UI.Headquarters.Services;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Headquarters.Services.Quartz;
using WB.UI.Shared.Web.Diagnostics;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.LoggingIntegration;
using WB.UI.Shared.Web.Mappings;
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
            AppDomain.CurrentDomain.AssemblyResolve += ResolveDataCollectionFix;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        
        private static Assembly ResolveDataCollectionFix(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("WB.Core.SharedKernels.DataCollection.Portable, Version="))
            {
                var assembly = Assembly.GetAssembly(typeof(Identity));
                return assembly;
            }

            return null;
        }

        public IConfiguration Configuration { get; }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            autofacKernel = new AutofacKernel(builder, container =>
            {
                InRootScopeExecutor.RootScope = container;
            });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var unitOfWorkConnectionSettings = BuildUnitOfWorkSettings(connectionString, Configuration);

            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                .Where(x => x?.Namespace?.Contains("Services.Impl") == true)
                .AsImplementedInterfaces();

            autofacKernel.Load(
                new NcqrsModule(),
                new SerilogLoggerModule(),
                new PostgresEventStoreModule(),
                new InfrastructureModule(),
                new DataCollectionSharedKernelModule(),
                new WebInterviewModule(Configuration),
                new DataCollectionSharedKernelModule(),
                new OrmModule(unitOfWorkConnectionSettings),
                new OwinSecurityModule(),
                new FileStorageModule(),
                new FileInfrastructureModule(),
                new DataExportModule(),
                GetHqBoundedContextModule(),
                new HeadquartersUiModule(Configuration),
                new ProductVersionModule(typeof(Startup).Assembly)
                );
        }

        public static UnitOfWorkConnectionSettings BuildUnitOfWorkSettings(string connectionString, 
            IConfiguration configuration = null)
        {
            var mappingAssemblies = new List<Assembly>
            {
                typeof(HeadquartersBoundedContextModule).Assembly,
                typeof(ProductVersionChangeMap).Assembly
            };

            var unitOfWorkConnectionSettings = new UnitOfWorkConnectionSettings
            {
                Configuration = configuration,
                ConnectionString = connectionString,
                ReadSideMappingAssemblies = mappingAssemblies,
                PlainMappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                },
                PlainStoreUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace),
                ReadSideUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_InitDb).Namespace),
                LogsUpgradeSettings = new DbUpgradeSettings(typeof(M201905171139_AddErrorsTable).Assembly,
                    typeof(M201905171139_AddErrorsTable).Namespace),
                UsersUpgradeSettings = DbUpgradeSettings.FromFirstMigration<M001_AddUsersHqIdentityModel>(),
                MigrateToPrimaryWorkspace = new DbUpgradeSettings(typeof(M202011131055_MoveOldSchemasToWorkspace).Assembly,
                    typeof(M202011131055_MoveOldSchemasToWorkspace).Namespace),
                EventStoreUpgradeSettings = new DbUpgradeSettings(typeof(WB.Persistence.Headquarters.Migrations.Events.M000_Init).Assembly,
                    typeof(WB.Persistence.Headquarters.Migrations.Events.M000_Init).Namespace),
                WorkspacesMigrationSettings = DbUpgradeSettings.FromFirstMigration<M202011191114_InitWorkspaces>(),
                SingleWorkspaceUpgradeSettings = DbUpgradeSettings.FromFirstMigration<WB.Persistence.Headquarters.Migrations.Workspace.M202011201421_InitSingleWorkspace>()
            };

            return unitOfWorkConnectionSettings;
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

            var synchronizationSettings = new SyncSettings(origin: Constants.SupervisorSynchronizationOrigin)
            {
                HumainIdMaxValue = Configuration.GetValue<int>("Headquarters:HumanIdMaxValue", 99_99_99_99)
            };

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
            services.AddCors(options =>
            {
                var redirectUri = Configuration["ExternalStorages:OAuth2:RedirectUri"];
                if (Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri))
                {
                    options.AddPolicy("export", b => b
                        .WithOrigins(redirectUri)
                        .AllowAnyHeader()
                        .WithMethods("POST")
                    );
                }
                else
                {
                    if(redirectUri == null) 
                        Log.Warning("No ExternalStorages:OAuth2:RedirectUri configuration provided");
                    else 
                        Log.Warning("Cannot parse {redirectUri} from ExternalStorages:OAuth2:RedirectUri", redirectUri);
                }
            });

            services.AddControllersWithViews().AddNewtonsoftJson(j =>
            {
                //j.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                j.SerializerSettings.Converters.Add(new StringEnumConverter());
                j.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
				options.ForwardLimit = 2;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.Name = "headquarters_session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddDataProtection().PersistWithPostgres(
                Configuration.GetConnectionString("DefaultConnection"));

            services.AddResponseCaching();
            services.AddResponseCompression();
            services.AddSignalR().AddNewtonsoftJsonProtocol();

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(Startup));

            services.Configure<CookiePolicyOptions>(options =>
            {
                var securityPolicy = Configuration.GetValue<Boolean?>("Policies:CookiesSecurePolicyAlways");
                
                if (securityPolicy.HasValue)
                    options.Secure = securityPolicy.Value ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
                else
                    options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            services.AddRazorPages();

            services.AddHqAuthorization(Configuration);
            services.AddDatabaseStoredExceptional(environment, Configuration);

            services.AddScoped<UnitOfWorkActionFilter>();
            services.AddScoped<InstallationFilter>();
            services.AddScoped<WorkspaceAccessActionFilter>();
            services.AddScoped<AntiForgeryFilter>();
            services.AddScoped<GlobalNotificationResultFilter>();
            services.AddTransient<ObservingNotAllowedActionFilter>();
            services.AddHeadquartersHealthCheck();

            services.AddTransient<ExportServiceApiConfigurator>();
            
            services.AddHttpClient();
            services.AddWorkspaceAwareHttpClient<IExportServiceApi,
                ExportServiceApiConfigurator,
                ExportServiceApiHttpHandler>(new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer()
            });

            services.AddWorkspaceAwareHttpClient<IDesignerApi, 
                DesignerApiConfigurator,
                DesignerRestServiceHandler>(new RefitSettings
                {
                    ContentSerializer = new DesignerContentSerializer()
                });
            

            services.AddScoped<IDesignerUserCredentials, DesignerUserCredentials>();

            services.AddGraphQL();

            FileStorageModule.Setup(services, Configuration);

            AddCompression(services);

            services.AddMvc(mvc =>
                {
                    mvc.Filters.Add<WorkspaceInfoFilter>();
                    mvc.Filters.AddService<UnitOfWorkActionFilter>(1);
                    mvc.Filters.AddService<InstallationFilter>(100);
                    mvc.Filters.AddService<WorkspaceAccessActionFilter>(150);
                    mvc.Filters.AddService<GlobalNotificationResultFilter>(200);
                    mvc.Filters.AddService<ObservingNotAllowedActionFilter>(300);
                    mvc.Filters.AddService<UpdateRequiredFilter>(400);

                    mvc.Filters.AddService<ExtraHeadersApiFilter>(500);
                    
                    //mvc.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });

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
            services.AddQuartzIntegration(Configuration,
                DbUpgradeSettings.FromFirstMigration<M201905151013_AddQuartzTables>());

            services.AddMediatR(typeof(Startup), typeof(HeadquartersBoundedContextModule));
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
            app.UseForwardedHeaders();

            app.UseExceptional();

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

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

            app.UseRequestLocalization(opt =>
            {
                opt.ApplyCurrentCultureToResponseHeaders = true;
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
                    new CultureInfo("zh"),
                    new CultureInfo("ro"),
                    new CultureInfo("cs")
                };
            });

            app.UseUnderConstruction();

            app.UseSerilogRequestLogging(o => o.Logger = app.ApplicationServices.GetService<ILogger>());
            
            app.UseWorkspaces();

            if (!env.IsDevelopment())
            {
                app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
                    appBuilder =>
                    {
                        appBuilder.UseStatusCodePagesWithReExecute("/error/{0}");
                        appBuilder.UseExceptionHandler("/error/500");
                    });
            }
            
            app.UseMetrics(Configuration);
            app.UseRouting();
            app.UseCors();

            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseResetPasswordRedirect();
            app.UseRedirectIntoWorkspace();

            app.UseAuthorization();

            app.UseSwagger();
            app.UseSession();
            app.UseResponseCompression();
            app.UseRequestDecompression();

            app.UseHqSwaggerUI();

            app.UseGraphQLApi();

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

                //endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapDefaultControllerRoute();

                endpoints.MapSwagger();

                endpoints.MapHub<WebInterview>("interview");
                endpoints.MapHub<SignalrDiagnosticHub>("signalrdiag",
                    options => { options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling; });

                endpoints.MapGet("/Index", ctx =>
                {
                    ctx.Response.Redirect("/");
                    return Task.CompletedTask;
                });

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
