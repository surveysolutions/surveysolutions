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
using WB.UI.Designer;
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

[assembly: PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
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

            var pdfSettings = settingsProvider.GetSection<PdfConfigSection>("pdf");

            var deskSettings = settingsProvider.GetSection<DeskConfigSection>("desk");

            var membershipSection = settingsProvider.GetSection<MembershipSection>("system.web/membership");
            var membershipSettings = membershipSection?.Providers[membershipSection.DefaultProvider].Parameters;
            var ormSettings = new UnitOfWorkConnectionSettings
            {
                ConnectionString = settingsProvider.ConnectionStrings["Postgres"].ConnectionString,
                PlainMappingAssemblies = new List<Assembly>
                {
                    typeof(DesignerBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                },
                PlainStorageSchemaName = "plainstore",
                ReadSideMappingAssemblies = new List<Assembly>(),
                PlainStoreUpgradeSettings = new DbUpgradeSettings(typeof(Migrations.PlainStore.M001_Init).Assembly, typeof(Migrations.PlainStore.M001_Init).Namespace)
            };


            var kernel = new NinjectKernel();
            kernel.Load(
                new EventFreeInfrastructureModule(),
                new InfrastructureModule(),
                new NcqrsModule(),
                new WebConfigurationModule(membershipSettings),
                new CaptchaModule(settingsProvider.AppSettings.Get("CaptchaService")),
                new NLogLoggingModule(),
                new OrmModule(ormSettings),
                new DesignerCommandDeserializationModule(),
                new DesignerBoundedContextModule(dynamicCompilerSettings),
                new QuestionnaireVerificationModule(),
                new MembershipModule(),
                new FileInfrastructureModule(),
                new ProductVersionModule(typeof(MvcApplication).Assembly)
                );
            kernel.Load(
                new DesignerRegistry(pdfSettings, deskSettings, settingsProvider.AppSettings.GetInt("QuestionnaireChangeHistoryLimit", 500)),
                new DesignerWebModule(),
                new NinjectWebCommonModule()
                );

            // init
            kernel.Init().Wait();

            return kernel.Kernel;
        }
    }
}
