﻿#nullable enable
using System;
using Android.App;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Supervisor.Views;
using Environment = System.Environment;

namespace WB.UI.Supervisor.Services.Implementation
{
    internal class SupervisorSettings : EnumeratorSettings, ISupervisorSettings
    {
        private readonly IPlainStorage<ApplicationSettingsView> settingsStorage;
        private readonly IPlainStorage<ApplicationWorkspaceSettingsView> workspaceSettingsStorage;
        private readonly IPlainStorage<SupervisorIdentity> usersStorage;
        private readonly IWorkspaceAccessor workspaceAccessor;

        public SupervisorSettings(IPlainStorage<ApplicationSettingsView> settingsStorage,
            IPlainStorage<ApplicationWorkspaceSettingsView> workspaceSettingsStorage,
            ISupervisorSyncProtocolVersionProvider syncProtocolVersionProvider,
            IQuestionnaireContentVersionProvider questionnaireContentVersionProvider,
            IPlainStorage<SupervisorIdentity> usersStorage,
            IFileSystemAccessor fileSystemAccessor,
            string backupFolder, string restoreFolder,
            IWorkspaceAccessor workspaceAccessor) : base(syncProtocolVersionProvider,
            questionnaireContentVersionProvider, fileSystemAccessor, backupFolder, restoreFolder)
        {
            this.settingsStorage = settingsStorage;
            this.workspaceSettingsStorage = workspaceSettingsStorage;
            this.usersStorage = usersStorage;
            this.workspaceAccessor = workspaceAccessor;
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
            HttpResponseTimeoutInSec = Application.Context.Resources?.GetInteger(Resource.Integer.HttpResponseTimeout) ?? 1200,
            EventChunkSize = Application.Context.Resources?.GetInteger(Resource.Integer.EventChunkSize),
            CommunicationBufferSize = Application.Context.Resources?.GetInteger(Resource.Integer.BufferSize) ?? 4096,
            ShowLocationOnMap = Application.Context.Resources?.GetBoolean(Resource.Boolean.ShowLocationOnMap) ?? true,
            DownloadUpdatesForInterviewerApp = Application.Context.Resources?.GetBoolean(Resource.Boolean.DownloadUpdatesForInterviewerApp),
        };

        private ApplicationWorkspaceSettingsView? currentWorkspaceSettings
        {
            get
            {
                var workspace = workspaceAccessor.GetCurrentWorkspaceName();
                if (workspace == null)
                    return null;
                
                return this.workspaceSettingsStorage.GetById(workspace) ?? new ApplicationWorkspaceSettingsView
                {
                    Id = workspace,
                };
            }
        }

        protected override EnumeratorSettingsView CurrentSettings => this.currentSettings;
        protected override EnumeratorWorkspaceSettingsView? CurrentWorkspaceSettings => this.currentWorkspaceSettings;
        public override int EventChunkSize => this.CurrentSettings.EventChunkSize.GetValueOrDefault(Application.Context.Resources?.GetInteger(Resource.Integer.EventChunkSize) ?? 1000);
        public override double GpsDesiredAccuracy => throw new NotImplementedException();
        public override bool VibrateOnError => false;
        public override bool ShowLocationOnMap => this.currentSettings.ShowLocationOnMap;

        public override int GpsReceiveTimeoutSec => throw new NotImplementedException();
        public void SetGpsResponseTimeout(int timeout) => throw new NotImplementedException();
        public void SetGpsDesiredAccuracy(double value) => throw new NotImplementedException();
        public void SetShowLocationOnMap(bool showLocationOnMap) => this.SaveCurrentSettings(settings => settings.ShowLocationOnMap = showLocationOnMap);

        public override void SetNotifications(bool notificationsEnabled) =>
            this.SaveCurrentSettings(settings => settings.NotificationsEnabled = notificationsEnabled);

        public string InterviewerApplicationsDirectory =>
            this.fileSystemAccessor.CombinePath(AndroidPathUtils.GetPathToInternalDirectory(), "patches");

        public bool DownloadUpdatesForInterviewerApp => this.currentSettings.DownloadUpdatesForInterviewerApp ?? true;

        public void SetDownloadUpdatesForInterviewerApp(bool downloadUpdatesForInterviewerApp) 
            => this.SaveCurrentSettings(settings => settings.DownloadUpdatesForInterviewerApp = downloadUpdatesForInterviewerApp);

        private void SaveCurrentSettings(Action<ApplicationSettingsView> onChanging)
        {
            var settings = this.currentSettings;
            onChanging(settings);
            SaveSettings(settings);
        }

        protected override void SaveSettings(EnumeratorSettingsView settings)
            => this.settingsStorage.Store((ApplicationSettingsView)settings);

        protected override void SaveSettings(EnumeratorWorkspaceSettingsView settings)
            => this.workspaceSettingsStorage.Store((ApplicationWorkspaceSettingsView)settings);
    }
}
