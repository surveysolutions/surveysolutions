using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.DeviceInfo;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Utils;
using Environment = System.Environment;

namespace WB.UI.Interviewer.Settings
{
    internal class InterviewerSettings : IInterviewerSettings
    {
        private readonly IPlainStorage<ApplicationSettingsView> settingsStorage;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;
        private readonly IQuestionnaireContentVersionProvider questionnaireContentVersionProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private PackageInfo appPackageInfo =>
            Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.MetaData);

        public InterviewerSettings(
            IPlainStorage<ApplicationSettingsView> settingsStorage, 
            ISyncProtocolVersionProvider syncProtocolVersionProvider, 
            IQuestionnaireContentVersionProvider questionnaireContentVersionProvider,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            IPlainStorage<InterviewView> interviewViewRepository, 
            IPlainStorage<QuestionnaireView> questionnaireViewRepository, 
            IFileSystemAccessor fileSystemAccessor,
            string backupFolder, 
            string restoreFolder)
        {
            this.settingsStorage = settingsStorage;
            this.syncProtocolVersionProvider = syncProtocolVersionProvider;
            this.questionnaireContentVersionProvider = questionnaireContentVersionProvider;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.fileSystemAccessor = fileSystemAccessor;
            this.BackupFolder = backupFolder;
            this.RestoreFolder = restoreFolder;
        }

        private ApplicationSettingsView CurrentSettings => this.settingsStorage.FirstOrDefault() ?? new ApplicationSettingsView
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

        public string Endpoint => this.CurrentSettings.Endpoint;
        public int EventChunkSize => this.CurrentSettings.EventChunkSize?? Application.Context.Resources.GetInteger(Resource.Integer.EventChunkSize);
        public bool VibrateOnError => this.CurrentSettings.VibrateOnError ?? Application.Context.Resources.GetBoolean(Resource.Boolean.VibrateOnError);
        public bool ShowVariables => false;
        public bool ShowLocationOnMap => this.CurrentSettings.ShowLocationOnMap.GetValueOrDefault(true);
        public TimeSpan Timeout => new TimeSpan(0, 0, this.CurrentSettings.HttpResponseTimeoutInSec);
        public int BufferSize => this.CurrentSettings.CommunicationBufferSize;
        public bool AcceptUnsignedSslCertificate => false;
        public int GpsReceiveTimeoutSec => this.CurrentSettings.GpsResponseTimeoutInSec;
        public double GpsDesiredAccuracy => this.CurrentSettings.GpsDesiredAccuracy.GetValueOrDefault(Application.Context.Resources.GetInteger(Resource.Integer.GpsDesiredAccuracy));

        public Version GetSupportedQuestionnaireContentVersion()
        {
            return questionnaireContentVersionProvider.GetSupportedQuestionnaireContentVersion();
        }

        public string GetDeviceId()
        {
            return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
        }

        private static string _userAgent = null;
        public string UserAgent
        {
            get
            {
                if (_userAgent != null) return _userAgent;

                var flags = new List<string>();
#if DEBUG
                flags.Add("DEBUG");
#endif
                flags.Add($"QuestionnarieVersion/{this.GetSupportedQuestionnaireContentVersion()}");
                _userAgent = $"{Application.Context.PackageName}/{this.GetApplicationVersionName()} ({string.Join(" ", flags)})";
                return _userAgent;
            }
        }
        public string GetApplicationVersionName() => this.appPackageInfo.VersionName;

        public string GetDeviceTechnicalInformation()
        {
            var interviewIds = string.Join(","+ Environment.NewLine, this.interviewViewRepository.LoadAll().Select(i => i.InterviewId));
            var questionnaireIds = string.Join(","+ Environment.NewLine, this.questionnaireViewRepository.LoadAll().Select(i => i.Id));

            return $"Version: {this.GetApplicationVersionName()} {Environment.NewLine}" +
                   $"SyncProtocolVersion: {this.syncProtocolVersionProvider.GetProtocolVersion()} {Environment.NewLine}" +
                   $"User: {GetUserInformation()} {Environment.NewLine}" +
                   $"Device model: {this.GetDeviceModel()} {Environment.NewLine}" +
                   $"Device type: {this.GetDeviceType()} {Environment.NewLine}" +
                   $"Android version: {GetAndroidVersion()} {Environment.NewLine}" +
                   $"DeviceId: {this.GetDeviceId()} {Environment.NewLine}" +
                   $"RAM: {GetRAMInformation()} {Environment.NewLine}" +
                   $"Disk: {GetDiskInformation()} {Environment.NewLine}" +
                   $"DBSize: {GetDataBaseSize()} {Environment.NewLine}" +
                   $"Endpoint: {this.Endpoint}{Environment.NewLine}" +
                   $"AcceptUnsignedSslCertificate: {this.AcceptUnsignedSslCertificate} {Environment.NewLine}" +
                   $"BufferSize: {this.BufferSize} {Environment.NewLine}" +
                   $"Timeout: {this.Timeout} {Environment.NewLine}" +
                   $"CurrentDateTime: {DateTime.Now} {Environment.NewLine}" +
                   $"QuestionnairesList: {questionnaireIds} {Environment.NewLine}" + 
                   $"EventChunkSize: {this.EventChunkSize} {Environment.NewLine}" +
                   $"VibrateOnError: {this.VibrateOnError} {Environment.NewLine}" +
                   $"InterviewsList: {interviewIds}";
        }

        public string GetDeviceModel() => CrossDeviceInfo.Current.Model;

        public string GetDeviceType() => CrossDeviceInfo.Current.Idiom.ToString();

        public string GetAndroidVersion() => Build.VERSION.Release;

        public void SetEventChunkSize(int eventChunkSize)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.EventChunkSize = eventChunkSize;
            });
        }

        public int GetApplicationVersionCode() => this.appPackageInfo.VersionCode;

        public void SetEndpoint(string endpoint)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.Endpoint = endpoint;
            });
        }

        public void SetHttpResponseTimeout(int timeout)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.HttpResponseTimeoutInSec = timeout;
            });
        }

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

        public void SetCommunicationBufferSize(int bufferSize)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.CommunicationBufferSize = bufferSize;
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

        public string BackupFolder { get; }

        public string RestoreFolder { get; }
        public string BandwidthTestUri { get; } = @"Dependencies/img/logo.png";

        public string InstallationFilePath => Application.Context.PackageManager
            .GetInstalledApplications(PackageInfoFlags.MetaData)
            .FirstOrDefault(x => x.PackageName == Application.Context.PackageName)
            .SourceDir;

        private void SaveCurrentSettings(Action<ApplicationSettingsView> onChanging)
        {
            var settings = this.CurrentSettings;
            onChanging(settings);
            this.settingsStorage.Store(settings);
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

        private string GetRAMInformation()
            => AndroidInformationUtils.GetRAMInformation();

        private string GetDiskInformation()
            => AndroidInformationUtils.GetDiskInformation();

        public string GetDataBaseSize() => 
            FileSizeUtils.SizeSuffix(this.fileSystemAccessor.GetDirectorySize(AndroidPathUtils.GetPathToInternalDirectory()));
    }
}
