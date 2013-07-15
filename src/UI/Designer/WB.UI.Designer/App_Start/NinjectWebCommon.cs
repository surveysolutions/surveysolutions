using System;
using System.Web;
using System.Web.Configuration;
using Main.Core;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using Ninject.Web.Common;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Questionnaire.ExportServices;
using WB.UI.Designer.App_Start;
using WB.UI.Designer.Code;
using WB.UI.Designer.CommandDeserialization;
using WB.UI.Shared.Web.CommandDeserialization;
using WebActivator;

[assembly: WebActivator.PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectWebCommon), "Stop")]

namespace WB.UI.Designer.App_Start
{
    using Microsoft.Practices.ServiceLocation;

    using NinjectAdapter;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        ///     Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        ///     Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        ///     Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new NLogLoggingModule(), new DesignerCommandDeserializationModule());
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
            kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);


            return kernel;
        }

        /// <summary>
        ///     Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            

            #warning TLK: delete this when NCQRS initialization moved to Global.asax
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization

            kernel.Load(new DesignerRegistry(
                            repositoryPath: AppSettings.Instance.RavenDocumentStore, isEmbeded: false));

            #warning TLK: move NCQRS initialization to Global.asax
            NcqrsInit.Init(kernel);

            kernel.Bind<IExportService>()
                  .ToConstant(new JsonExportService(kernel.Get<IReadSideRepositoryReader<QuestionnaireDocument>>()));

            kernel.Load<MembershipModule>();
            kernel.Load<MainModule>();
        }
    }
}