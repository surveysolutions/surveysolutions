using System.Configuration;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using NConfig;
using Ninject;
using System;
using System.Windows;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Raven;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.Core.SharedKernels.QuestionnaireVerification;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.Synchronization;

namespace CapiDataGenerator
{
    public partial class App : Application
    {
        private bool _setupComplete;

        private void DoSetup()
        {
            SetupNConfig();

            var presenter = new MvxSimpleWpfViewPresenter(MainWindow);
            var setup = new Setup(Dispatcher, presenter);
            setup.Initialize();

            var ravenSupervisorSettings = new RavenConnectionSettings(ConfigurationManager.AppSettings["Supervisor.Raven.DocumentStore"], 
                isEmbedded: false,
                eventsDatabase: ConfigurationManager.AppSettings["Supervisor.Raven.Databases.Events"],
                viewsDatabase: ConfigurationManager.AppSettings["Supervisor.Raven.Databases.Views"],
                plainDatabase: ConfigurationManager.AppSettings["Supervisor.Raven.Databases.PlainStorage"]);

            var ravenHeadquartersSettings = new RavenConnectionSettings(ConfigurationManager.AppSettings["Headquarters.Raven.DocumentStore"],
                isEmbedded: false,
                eventsDatabase: ConfigurationManager.AppSettings["Headquarters.Raven.Databases.Events"],
                viewsDatabase: ConfigurationManager.AppSettings["Headquarters.Raven.Databases.Views"],
                plainDatabase: ConfigurationManager.AppSettings["Headquarters.Raven.Databases.PlainStorage"]);

            new StandardKernel(
                new RavenReadSideInfrastructureModule(ravenSupervisorSettings),
                new SynchronizationModule(AppDomain.CurrentDomain.BaseDirectory, new SyncSettings(reevaluateInterviewWhenSynchronized: true)),
                new RavenPlainStorageInfrastructureModule(ravenSupervisorSettings),
                new CapiDataGeneratorRegistry(),
                new NLogLoggingModule(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false),
                new ExpressionProcessorModule(),
                new QuestionnaireVerificationModule(),
                new FileInfrastructureModule(),
                new MainModelModule(ravenHeadquartersSettings));

            var start = Mvx.Resolve<IMvxAppStart>();
            start.Start();

            _setupComplete = true;
        }

        protected override void OnActivated(EventArgs e)
        {
            if (!_setupComplete)
                DoSetup();

            base.OnActivated(e);
        }

        private void SetupNConfig()
        {
            NConfigurator.UsingFiles(@"Configuration\WB.Tools.CapiDataGenerator.config").SetAsSystemDefault();
        }
    }
}