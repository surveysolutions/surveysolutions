using System;
using Android.App;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Supervisor.Views;
using Environment = System.Environment;

namespace WB.UI.Supervisor.Services.Implementation
{
    internal class SupervisorSettings : EnumeratorSettings, ISupervisorSettings
    {
        private readonly IPlainStorage<ApplicationSettingsView> settingsStorage;
        private readonly IPlainStorage<SupervisorIdentity> usersStorage;

        public SupervisorSettings(IPlainStorage<ApplicationSettingsView> settingsStorage,
            ISupervisorSyncProtocolVersionProvider syncProtocolVersionProvider,
            IQuestionnaireContentVersionProvider questionnaireContentVersionProvider,
            IPlainStorage<SupervisorIdentity> usersStorage,
            IFileSystemAccessor fileSystemAccessor,
            string backupFolder, string restoreFolder) : base(syncProtocolVersionProvider,
            questionnaireContentVersionProvider, fileSystemAccessor, backupFolder, restoreFolder)
        {
            this.settingsStorage = settingsStorage;
            this.usersStorage = usersStorage;
        }


        private string GetUserInformation()
        {
            var currentStoredUser = this.usersStorage.FirstOrDefault();

            return currentStoredUser != null ? $"{currentStoredUser.Name}: {currentStoredUser.Id}" : "NONE";
        }

        protected override string GetExternalInformation()
        {
            return $"User: {GetUserInformation()} {Environment.NewLine}";
        }

        private ApplicationSettingsView currentSettings => this.settingsStorage.FirstOrDefault() ?? new ApplicationSettingsView
        {
            Id = "settings",
            Endpoint = string.Empty,
            HttpResponseTimeoutInSec = Application.Context.Resources.GetInteger(Resource.Integer.HttpResponseTimeout),
            EventChunkSize = Application.Context.Resources.GetInteger(Resource.Integer.EventChunkSize),
            CommunicationBufferSize = Application.Context.Resources.GetInteger(Resource.Integer.BufferSize),
            ShowLocationOnMap = Application.Context.Resources.GetBoolean(Resource.Boolean.ShowLocationOnMap)
        };

        protected override EnumeratorSettingsView CurrentSettings => this.currentSettings;
        public override int EventChunkSize => this.CurrentSettings.EventChunkSize.GetValueOrDefault(Application.Context.Resources.GetInteger(Resource.Integer.EventChunkSize));
        public override double GpsDesiredAccuracy => throw new NotImplementedException();
        public override bool VibrateOnError => false;
        public override bool ShowLocationOnMap => this.currentSettings.ShowLocationOnMap;
        public override int GpsReceiveTimeoutSec => throw new NotImplementedException();
        public void SetGpsResponseTimeout(int timeout) => throw new NotImplementedException();
        public void SetGpsDesiredAccuracy(double value) => throw new NotImplementedException();
        public void SetShowLocationOnMap(bool showLocationOnMap) => this.SaveCurrentSettings(settings => settings.ShowLocationOnMap = showLocationOnMap);

        public string InterviewerAppPatchesDirectory =>
            this.fileSystemAccessor.CombinePath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "patches");

        private void SaveCurrentSettings(Action<ApplicationSettingsView> onChanging)
        {
            var settings = this.currentSettings;
            onChanging(settings);
            SaveSettings(settings);
        }

        protected override void SaveSettings(EnumeratorSettingsView settings)
            => this.settingsStorage.Store((ApplicationSettingsView)settings);
    }
}
