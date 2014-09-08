using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using NConfig;
using System;
using System.Windows;

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