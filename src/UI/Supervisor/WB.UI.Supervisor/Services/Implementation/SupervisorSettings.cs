﻿using System;
using Android.App;
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
    internal class SupervisorSettings : EnumeratorSettings
    {
        private readonly IPlainStorage<ApplicationSettingsView> settingsStorage;
        private readonly IPlainStorage<SupervisorIdentity> usersStorage;

        public SupervisorSettings(IPlainStorage<ApplicationSettingsView> settingsStorage,
            ISyncProtocolVersionProvider syncProtocolVersionProvider,
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
            CommunicationBufferSize = Application.Context.Resources.GetInteger(Resource.Integer.BufferSize)
        };

        protected override EnumeratorSettingsView CurrentSettings => this.currentSettings;
        public override double GpsDesiredAccuracy => throw new NotImplementedException();
        public override int EventChunkSize => throw new NotImplementedException();
        public override bool VibrateOnError => throw new NotImplementedException();
        public override bool ShowLocationOnMap => throw new NotImplementedException();
        public override int GpsReceiveTimeoutSec => throw new NotImplementedException();

        protected override void SaveSettings(EnumeratorSettingsView settings)
            => this.settingsStorage.Store((ApplicationSettingsView)settings);
    }
}
