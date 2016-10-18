using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http.Filters;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Designer.App_Start;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.ConfigurationManager;
using WB.UI.Designer.CommandDeserialization;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WB.UI.Shared.Web.Versions;
using WebActivatorEx;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectWebCommon), "Stop")]

namespace WB.UI.Designer.App_Start
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        private static IKernel CreateKernel()
        {
            // HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization

            var dynamicCompilerSettings = (ICompilerSettings)WebConfigurationManager.GetSection("dynamicCompilerSettingsGroup");

            string appDataDirectory = WebConfigurationManager.AppSettings["DataStorePath"];
            if (appDataDirectory.StartsWith("~/") || appDataDirectory.StartsWith(@"~\"))
            {
                appDataDirectory = HostingEnvironment.MapPath(appDataDirectory);
            }

            var cacheSettings = new ReadSideCacheSettings(
                enableEsentCache: WebConfigurationManager.AppSettings.GetBool("Esent.Cache.Enabled", @default: true),
                esentCacheFolder: Path.Combine(appDataDirectory, WebConfigurationManager.AppSettings.GetString("Esent.Cache.Folder", @default: @"Temp\EsentCache")),
                cacheSizeInEntities: WebConfigurationManager.AppSettings.GetInt("ReadSide.CacheSize", @default: 1024),
                storeOperationBulkSize: WebConfigurationManager.AppSettings.GetInt("ReadSide.BulkSize", @default: 512));

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = WebConfigurationManager.ConnectionStrings["Postgres"].ConnectionString,
                SchemaName = "plainstore",
                DbUpgradeSettings = new DbUpgradeSettings(typeof(Migrations.PlainStore.M001_Init).Assembly, typeof(Migrations.PlainStore.M001_Init).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(DesignerBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                }
            };

            var pdfSettings = (PdfConfigSection)WebConfigurationManager.GetSection("pdf");

            var deskSettings = (DeskConfigSection)WebConfigurationManager.GetSection("desk");

            var kernel = new StandardKernel(
                new ServiceLocationModule(),
                new EventFreeInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(),
                new PostgresKeyValueModule(cacheSettings),
                new PostgresPlainStorageModule(postgresPlainStorageSettings),
                new DesignerRegistry(pdfSettings, deskSettings),
                new DesignerCommandDeserializationModule(),
                new DesignerBoundedContextModule(dynamicCompilerSettings),
                new QuestionnaireVerificationModule(),
                new MembershipModule(),
                new MainModule(),
                new FileInfrastructureModule(),
                new ProductVersionModule(typeof(MvcApplication).Assembly)
                );

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<ApiValidationAntiForgeryTokenAttribute>()
                .WithConstructorArgument("tokenVerifier", new ApiValidationAntiForgeryTokenVerifier());

            kernel.Bind<ISettingsProvider>().To<DesignerSettingsProvider>().InSingletonScope();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            kernel.Bind<IAuthenticationService>().To<AuthenticationService>();
            kernel.Bind<IRecaptchaService>().To<RecaptchaService>();
            kernel.Bind<QuestionnaireDowngradeService>().ToSelf();

            return kernel;
        }
    }
}