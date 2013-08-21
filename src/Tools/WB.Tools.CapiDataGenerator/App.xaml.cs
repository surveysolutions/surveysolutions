using System.Configuration;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using NConfig;
using Ninject;
using System;
using System.Windows;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure.Raven;
using WB.Core.Synchronization;
using WB.Supervisor.CompleteQuestionnaireDenormalizer;

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

            var ravenSettings = new RavenConnectionSettings(ConfigurationManager.AppSettings["Raven.DocumentStore"]);

            new StandardKernel(
                new RavenReadSideInfrastructureModule(ravenSettings),
                new SynchronizationModule(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)),
                new CapiDataGeneratorRegistry(),
                new NLogLoggingModule(),
                new CompleteQuestionnarieDenormalizerModule(),
                new MainModelModule());

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