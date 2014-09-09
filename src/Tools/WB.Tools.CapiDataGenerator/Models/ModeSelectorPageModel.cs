using System;
using System.Collections.ObjectModel;
using System.Configuration;
using CapiDataGenerator;
using Cirrious.MvvmCross.ViewModels;
using Ninject;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.Core.SharedKernels.QuestionnaireVerification;
using WB.Core.Synchronization;

namespace WB.Tools.CapiDataGenerator.Models
{
    public class ModeSelectorPageModel : MvxViewModel
    {

        public MvxCommand SelectCommand { get; set; }

        private GenerationMode? selectedWorkingMode = null;
        public GenerationMode? SelectedWorkingMode
        {
            get
            {
                return selectedWorkingMode;
            }
            set
            {
                selectedWorkingMode = value;
                RaisePropertyChanged(() => SelectedWorkingMode);
            }
        }

        private ObservableCollection<string> workingModeList = new ObservableCollection<string>();
        public ObservableCollection<string> WorkingModeList
        {
            get
            {
                return workingModeList;
            }
            set
            {
                workingModeList = value;
                RaisePropertyChanged(() => WorkingModeList);
            }
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            this.WorkingModeList = new ObservableCollection<string>(Enum.GetNames(typeof(GenerationMode)));
        }

        public ModeSelectorPageModel()
        {
            this.SelectCommand = new MvxCommand(this.Select, () => true);
        }

        public void Select()
        {
            if (this.SelectedWorkingMode == null)
                return;

            this.Init();

            AppSettings.Instance.CurrentMode = this.SelectedWorkingMode.Value;

            ShowViewModel<MainPageModel>();
        }

        private void Init()
        {
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

            var synchronizationSettings = new SyncSettings(reevaluateInterviewWhenSynchronized: true,
                appDataDirectory: AppDomain.CurrentDomain.BaseDirectory,
                incomingCapiPackagesDirectoryName: ConfigurationManager.AppSettings["Synchronization.IncomingCapiPackagesDirectory"],
                incomingCapiPackagesWithErrorsDirectoryName: ConfigurationManager.AppSettings["Synchronization.IncomingCapiPackagesWithErrorsDirectory"],
                incomingCapiPackageFileNameExtension: ConfigurationManager.AppSettings["Synchronization.IncomingCapiPackageFileNameExtension"]);

            new StandardKernel(
                new RavenReadSideInfrastructureModule(ravenSupervisorSettings),
                new SynchronizationModule(synchronizationSettings),
                new RavenPlainStorageInfrastructureModule(ravenSupervisorSettings),
                new CapiDataGeneratorRegistry(),
                new NLogLoggingModule(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false),
                new ExpressionProcessorModule(),
                new QuestionnaireVerificationModule(),
                new FileInfrastructureModule(),
                new MainModelModule(ravenHeadquartersSettings, ravenSupervisorSettings));
        }

    }
}
