using System;
using System.Web;
using System.Web.Configuration;
using Main.Core.View;
using Microsoft.Owin.Security;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.PasswordPolicy;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Views;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Raven;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.Core.SharedKernels.QuestionnaireVerification;
using WB.UI.Headquarters;
using WB.UI.Headquarters.Models;
using WB.Core.SharedKernels.DataCollection;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace WB.UI.Headquarters
{
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            MvcApplication.Initialize();
            var ravenConnectionSettings = new RavenConnectionSettings(
                WebConfigurationManager.AppSettings["Raven.DocumentStore"], 
                isEmbedded: false,
                username: WebConfigurationManager.AppSettings["Raven.Username"],
                password: WebConfigurationManager.AppSettings["Raven.Password"],
                eventsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Events"],
                viewsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                plainDatabase: WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"]);

            var kernel = new StandardKernel(
                new ServiceLocationModule(),
                new NLogLoggingModule(AppDomain.CurrentDomain.BaseDirectory),
                new RavenPlainStorageInfrastructureModule(ravenConnectionSettings),
                new RavenWriteSideInfrastructureModule(ravenConnectionSettings),
                new RavenReadSideInfrastructureModule(ravenConnectionSettings),
                new DataCollectionSharedKernelModule(),
                new QuestionnaireVerificationModule(),
                new ExpressionProcessorModule(),
                new HeadquartersRegistry(),
                new PasswordPolicyModule(int.Parse(WebConfigurationManager.AppSettings["MinPasswordLength"]), WebConfigurationManager.AppSettings["PasswordPattern"]),
                new AuthenticationModule(),
                new HeadquartersBoundedContextModule(int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Major"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Minor"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Patch"])),
                new CqrsModule());

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            kernel.Bind<IAuthenticationManager>().ToMethod(ctx => HttpContext.Current.GetOwinContext().Authentication);

            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<SslSettings>().ToMethod((context) => new SslSettings
            {
                AcceptUnsignedCertificate = bool.Parse(WebConfigurationManager.AppSettings["AcceptUnsignedCertificate"])
            }).InTransientScope();
        }
    }
}
