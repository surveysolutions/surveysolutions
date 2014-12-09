﻿using System;
using System.Collections.ObjectModel;
using System.Configuration;
using CapiDataGenerator;
using Cirrious.MvvmCross.ViewModels;
using Ninject;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Modules;

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
                eventsDatabase: ConfigurationManager.AppSettings["Supervisor.Raven.Databases.Events"],
                viewsDatabase: ConfigurationManager.AppSettings["Supervisor.Raven.Databases.Views"],
                plainDatabase: ConfigurationManager.AppSettings["Supervisor.Raven.Databases.PlainStorage"]);

            var ravenHeadquartersSettings = new RavenConnectionSettings(ConfigurationManager.AppSettings["Headquarters.Raven.DocumentStore"],
                eventsDatabase: ConfigurationManager.AppSettings["Headquarters.Raven.Databases.Events"],
                viewsDatabase: ConfigurationManager.AppSettings["Headquarters.Raven.Databases.Views"],
                plainDatabase: ConfigurationManager.AppSettings["Headquarters.Raven.Databases.PlainStorage"]);

            var synchronizationSettings = new SyncSettings(reevaluateInterviewWhenSynchronized: true,
                appDataDirectory: AppDomain.CurrentDomain.BaseDirectory,
                incomingCapiPackagesDirectoryName: ConfigurationManager.AppSettings["Synchronization.IncomingCapiPackagesDirectory"],
                incomingCapiPackagesWithErrorsDirectoryName: ConfigurationManager.AppSettings["Synchronization.IncomingCapiPackagesWithErrorsDirectory"],
                incomingCapiPackageFileNameExtension: ConfigurationManager.AppSettings["Synchronization.IncomingCapiPackageFileNameExtension"]);

            var basePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            new StandardKernel(
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new RavenReadSideInfrastructureModule(ravenSupervisorSettings, basePath),
                new SynchronizationModule(synchronizationSettings),
                new RavenPlainStorageInfrastructureModule(ravenSupervisorSettings),
                new CapiDataGeneratorRegistry(),
                new NLogLoggingModule(basePath),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false, basePath: basePath),
                new FileInfrastructureModule(),
                new MainModelModule(ravenHeadquartersSettings, ravenSupervisorSettings));
        }

    }
}
