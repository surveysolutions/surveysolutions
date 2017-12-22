using System.Collections.Generic;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Hosting;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using WB.Core.BoundedContexts.Designer;
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
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Extensions;
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
            var settingsProvider = new SettingsProvider();

            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization

            var dynamicCompilerSettings = settingsProvider.GetSection<DynamicCompilerSettingsGroup>("dynamicCompilerSettingsGroup");

            string appDataDirectory = settingsProvider.AppSettings["DataStorePath"];
            if (appDataDirectory.StartsWith("~/") || appDataDirectory.StartsWith(@"~\"))
            {
                appDataDirectory = HostingEnvironment.MapPath(appDataDirectory);
            }

            var cacheSettings = new ReadSideCacheSettings(
                cacheSizeInEntities: settingsProvider.AppSettings.GetInt("ReadSide.CacheSize", @default: 1024),
                storeOperationBulkSize: settingsProvider.AppSettings.GetInt("ReadSide.BulkSize", @default: 512));

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = settingsProvider.ConnectionStrings["Postgres"].ConnectionString,
                SchemaName = "plainstore",
                DbUpgradeSettings = new DbUpgradeSettings(typeof(Migrations.PlainStore.M001_Init).Assembly, typeof(Migrations.PlainStore.M001_Init).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(DesignerBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                }
            };

            var pdfSettings = settingsProvider.GetSection<PdfConfigSection>("pdf");

            var deskSettings = settingsProvider.GetSection<DeskConfigSection>("desk");

            var membershipSection = settingsProvider.GetSection<MembershipSection>("system.web/membership");
            var membershipSettings = membershipSection?.Providers[membershipSection.DefaultProvider].Parameters;

            var kernel = new StandardKernel(
                new ServiceLocationModule(),
                new EventFreeInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(membershipSettings).AsNinject(),
                new CaptchaModule(settingsProvider.AppSettings.Get("CaptchaService")).AsNinject(),
                new NLogLoggingModule().AsNinject(),
                new PostgresKeyValueModule(cacheSettings).AsNinject(),
                new PostgresPlainStorageModule(postgresPlainStorageSettings).AsNinject(),
                new DesignerRegistry(pdfSettings, deskSettings, settingsProvider.AppSettings.GetInt("QuestionnaireChangeHistoryLimit", 500)).AsWebNinject(),
                new DesignerCommandDeserializationModule().AsNinject(),
                new DesignerBoundedContextModule(dynamicCompilerSettings).AsNinject(),
                new QuestionnaireVerificationModule().AsNinject(),
                new MembershipModule().AsNinject(),
                new MainModule().AsWebNinject(),
                new FileInfrastructureModule().AsNinject(),
                new ProductVersionModule(typeof(MvcApplication).Assembly).AsNinject(),
                new NinjectWebCommonModule().AsWebNinject()
                );

            return kernel;
        }
    }
}