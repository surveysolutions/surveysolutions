using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Utils;
using Environment = System.Environment;

namespace WB.UI.Shared.Enumerator.Services
{
    public abstract class EnumeratorSettings : IEnumeratorSettings
    {
        private readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;
        private readonly IQuestionnaireContentVersionProvider questionnaireContentVersionProvider;
        protected readonly IFileSystemAccessor fileSystemAccessor;

        private PackageInfo appPackageInfo =>
            Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.MetaData);

        protected EnumeratorSettings(
            ISyncProtocolVersionProvider syncProtocolVersionProvider, 
            IQuestionnaireContentVersionProvider questionnaireContentVersionProvider,
            IFileSystemAccessor fileSystemAccessor,
            string backupFolder, 
            string restoreFolder)
        {
            this.syncProtocolVersionProvider = syncProtocolVersionProvider;
            this.questionnaireContentVersionProvider = questionnaireContentVersionProvider;
            this.fileSystemAccessor = fileSystemAccessor;
            this.BackupFolder = backupFolder;
            this.RestoreFolder = restoreFolder;
        }

        protected abstract EnumeratorSettingsView CurrentSettings { get; }

        public string Endpoint => this.CurrentSettings.Endpoint;
        public long? LastHqSyncTimestamp => this.CurrentSettings.LastHqSyncTimestamp;

        public DateTime? LastSync => this.CurrentSettings.LastSync;

        public bool? LastSyncSucceeded => this.CurrentSettings.LastSyncSucceeded;
        public string LastOpenedMapName => this.CurrentSettings.LastOpenedMapName;

        public abstract bool VibrateOnError { get; }
        public abstract bool ShowLocationOnMap { get; }
        public abstract int GpsReceiveTimeoutSec { get; }
        public abstract double GpsDesiredAccuracy { get; }
        public abstract int EventChunkSize { get; }

        public bool ShowVariables => false;
        public bool ShowAnswerTime => false;
        public bool AcceptUnsignedSslCertificate => false;
        public TimeSpan Timeout => new TimeSpan(0, 0, this.CurrentSettings.HttpResponseTimeoutInSec);
        public int BufferSize => this.CurrentSettings.CommunicationBufferSize;
        
        
        public Version GetSupportedQuestionnaireContentVersion() 
            => questionnaireContentVersionProvider.GetSupportedQuestionnaireContentVersion();

        public string GetDeviceId()
        {
            return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
        }

        private static string userAgent;
        public string UserAgent
        {
            get
            {
                if (userAgent != null) return userAgent;

                var flags = new List<string>();
#if DEBUG
                flags.Add("DEBUG");
#endif
                flags.Add($"QuestionnaireVersion/{this.GetSupportedQuestionnaireContentVersion()}");
                // Be very careful when changing this. CheckCompatibility method parses this
                userAgent = $"{Application.Context.PackageName}/{this.GetApplicationVersionName()} ({string.Join(" ", flags)})";
                return userAgent;
            }
        }

        public int MaxDegreeOfParallelism { get; } = 4;

        public EnumeratorApplicationType ApplicationType =>
            this.GetApplicationVersionName()?.ToLower()?.Contains(@"maps") ?? false
                ? EnumeratorApplicationType.WithMaps
                : EnumeratorApplicationType.WithoutMaps;

        public bool Encrypted => this.CurrentSettings.Encrypted ?? false;
        public void SetEncrypted(bool encrypted) => this.SaveCurrentSettings(s => s.Encrypted = encrypted);
        public bool DashboardViewsUpdated => this.CurrentSettings.DashboardViewsUpdated ?? false;
        public void SetDashboardViewsUpdated(bool updated) => this.SaveCurrentSettings(s => s.DashboardViewsUpdated = updated);

        public bool NotificationsEnabled => this.CurrentSettings.NotificationsEnabled ?? true;
        public virtual void SetNotifications(bool notificationsEnabled) => this.SaveCurrentSettings(s => s.NotificationsEnabled = notificationsEnabled);
        public void MarkSyncStart()
        {
            this.SaveCurrentSettings(settings => {
                settings.LastSync = DateTime.Now;
                settings.LastSyncSucceeded = false;
            });
        }

        public void MarkSyncSucceeded()
        {
            this.SaveCurrentSettings(settings => {
                settings.LastSync = DateTime.Now;
                settings.LastSyncSucceeded = true;
            });
        }
        public void SetLastOpenedMapName(string mapName)
        {
            this.SaveCurrentSettings(settings => {
                settings.LastOpenedMapName = mapName;
            });
        }

        public string GetApplicationVersionName() => this.appPackageInfo.VersionName;

        public string GetDeviceTechnicalInformation() => $"Version: {this.GetApplicationVersionName()} {Environment.NewLine}" +
                                                         $"SyncProtocolVersion: {this.syncProtocolVersionProvider.GetProtocolVersion()} {Environment.NewLine}" +
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
                                                         $"EventChunkSize: {this.EventChunkSize} {Environment.NewLine}" +
                                                         this.GetExternalInformation();

        protected abstract string GetExternalInformation();

        public string GetDeviceModel() => Xamarin.Essentials.DeviceInfo.Model;

        public string GetDeviceType() => Xamarin.Essentials.DeviceInfo.Idiom.ToString();

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

        public void SetLastHqSyncTimestamp(long? timestamp)
        {
            this.SaveCurrentSettings(settings => { settings.LastHqSyncTimestamp = timestamp; });
        }

        public void SetHttpResponseTimeout(int timeout)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.HttpResponseTimeoutInSec = timeout;
            });
        }

        public void SetCommunicationBufferSize(int bufferSize)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.CommunicationBufferSize = bufferSize;
            });
        }

        public string BackupFolder { get; }

        public string RestoreFolder { get; }
        public string BandwidthTestUri { get; } = @".hc";

        public string InstallationFilePath => Application.Context.PackageManager
            .GetInstalledApplications(PackageInfoFlags.MetaData)
            .FirstOrDefault(x => x.PackageName == Application.Context.PackageName)
            .SourceDir;

        private void SaveCurrentSettings(Action<EnumeratorSettingsView> onChanging)
        {
            var settings = this.CurrentSettings;
            onChanging(settings);
            this.SaveSettings(settings);
        }

        protected abstract void SaveSettings(EnumeratorSettingsView settings);

        private string GetRAMInformation()
            => AndroidInformationUtils.GetRAMInformation();

        private string GetDiskInformation()
            => AndroidInformationUtils.GetDiskInformation();

        public string GetDataBaseSize() => 
            FileSizeUtils.SizeSuffix(this.fileSystemAccessor.GetDirectorySize(AndroidPathUtils.GetPathToInternalDirectory()));

        public bool PartialSynchronizationEnabled => this.CurrentSettings.PartialSynchronizationEnabled ?? false;

        public void SetPartialSynchronizationEnabled(bool enable)
        {
            this.SaveCurrentSettings(settings => { settings.PartialSynchronizationEnabled = enable; });
        }

        public List<QuestionnaireIdentity> QuestionnairesInWebMode => 
            string.IsNullOrWhiteSpace(this.CurrentSettings.QuestionnairesInWebMode)
                ? new List<QuestionnaireIdentity>()
                : this.CurrentSettings.QuestionnairesInWebMode.Split(',')
                    .Select(QuestionnaireIdentity.Parse).ToList();

        public string WebInterviewUriTemplate => this.CurrentSettings.WebInterviewUriTemplate;

        public void SetQuestionnaireInWebMode(List<string> questionnairesInWebMode)
        {
            this.SaveCurrentSettings(settings => settings.QuestionnairesInWebMode = string.Join(",", questionnairesInWebMode));
        }

        public void SetWebInterviewUrlTemplate(string webInterviewUriTemplate)
        {
            this.SaveCurrentSettings(settings => settings.WebInterviewUriTemplate = webInterviewUriTemplate);
        }
    }
}
