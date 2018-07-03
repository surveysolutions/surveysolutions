using System;
using System.Linq;
using Android.App;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Services;
using Environment = System.Environment;

namespace WB.UI.Interviewer.Settings
{
    internal class InterviewerSettings : EnumeratorSettings, IInterviewerSettings
    {
        private readonly IPlainStorage<ApplicationSettingsView> settingsStorage;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;

        public InterviewerSettings(IPlainStorage<ApplicationSettingsView> settingsStorage,
            IInterviewerSyncProtocolVersionProvider syncProtocolVersionProvider,
            IQuestionnaireContentVersionProvider questionnaireContentVersionProvider,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository, 
            IFileSystemAccessor fileSystemAccessor,
            string backupFolder, string restoreFolder) : base(syncProtocolVersionProvider,
            questionnaireContentVersionProvider, fileSystemAccessor, backupFolder, restoreFolder)
        {
            this.settingsStorage = settingsStorage;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.questionnaireViewRepository = questionnaireViewRepository;
        }


        private string GetUserInformation()
        {
            var currentStoredUser = this.interviewersPlainStorage.FirstOrDefault();
            if (currentStoredUser != null)
            {
                return $"{currentStoredUser.Name}: {currentStoredUser.Id}";
            }
            return "NONE";
        }

        protected override string GetExternalInformation()
        {
            var interviewIds = string.Join("," + Environment.NewLine, this.interviewViewRepository.LoadAll().Select(i => i.InterviewId));
            var questionnaireIds = string.Join("," + Environment.NewLine, this.questionnaireViewRepository.LoadAll().Select(i => i.Id));

            return $"User: {GetUserInformation()} {Environment.NewLine}" +
                   $"VibrateOnError: {this.VibrateOnError} {Environment.NewLine}" +
                   $"QuestionnairesList: {questionnaireIds} {Environment.NewLine}" +
                   $"InterviewsList: {interviewIds}";
        }

        private ApplicationSettingsView currentSettings => this.settingsStorage.FirstOrDefault() ?? new ApplicationSettingsView
        {
            Id = "settings",
            Endpoint = string.Empty,
            HttpResponseTimeoutInSec = Application.Context.Resources.GetInteger(Resource.Integer.HttpResponseTimeout),
            EventChunkSize = Application.Context.Resources.GetInteger(Resource.Integer.EventChunkSize),
            CommunicationBufferSize = Application.Context.Resources.GetInteger(Resource.Integer.BufferSize),
            GpsResponseTimeoutInSec = Application.Context.Resources.GetInteger(Resource.Integer.GpsReceiveTimeoutSec),
            GpsDesiredAccuracy = Application.Context.Resources.GetInteger(Resource.Integer.GpsDesiredAccuracy),
            VibrateOnError = Application.Context.Resources.GetBoolean(Resource.Boolean.VibrateOnError)
        };

        protected override EnumeratorSettingsView CurrentSettings => this.currentSettings;

        public override bool VibrateOnError => this.currentSettings.VibrateOnError ?? Application.Context.Resources.GetBoolean(Resource.Boolean.VibrateOnError);
        public override double GpsDesiredAccuracy => this.currentSettings.GpsDesiredAccuracy.GetValueOrDefault(Application.Context.Resources.GetInteger(Resource.Integer.GpsDesiredAccuracy));
        public override bool ShowLocationOnMap => this.currentSettings.ShowLocationOnMap.GetValueOrDefault(true);
        public override int GpsReceiveTimeoutSec => this.currentSettings.GpsResponseTimeoutInSec;
        public override int EventChunkSize => this.CurrentSettings.EventChunkSize.GetValueOrDefault(Application.Context.Resources.GetInteger(Resource.Integer.EventChunkSize));


        public void SetGpsResponseTimeout(int timeout)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.GpsResponseTimeoutInSec = timeout;
            });
        }

        public void SetGpsDesiredAccuracy(double value)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.GpsDesiredAccuracy = value;
            });
        }


        public void SetVibrateOnError(bool vibrateOnError)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.VibrateOnError = vibrateOnError;
            });
        }

        public void SetShowLocationOnMap(bool showLocationOnMap)
        {
            this.SaveCurrentSettings(settings => settings.ShowLocationOnMap = showLocationOnMap);
        }

        private void SaveCurrentSettings(Action<ApplicationSettingsView> onChanging)
        {
            var settings = this.currentSettings;
            onChanging(settings);
            
        }

        protected override void SaveSettings(EnumeratorSettingsView settings)
            => this.settingsStorage.Store((ApplicationSettingsView)settings);
    }
}
