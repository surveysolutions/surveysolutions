using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using PCLStorage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

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
        
        private readonly string backupFolder;
        private readonly string restoreFolder;

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
            this.backupFolder = backupFolder;
            this.restoreFolder = restoreFolder;
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
        public TimeSpan Timeout => new TimeSpan(0, 0, this.CurrentSettings.HttpResponseTimeoutInSec);
        public int BufferSize => this.CurrentSettings.CommunicationBufferSize;
        public bool AcceptUnsignedSslCertificate => false;
        public int GpsReceiveTimeoutSec => this.CurrentSettings.GpsResponseTimeoutInSec;
        public double GpsDesiredAccuracy => this.CurrentSettings.GpsDesiredAccuracy.GetValueOrDefault((double)Application.Context.Resources.GetInteger(Resource.Integer.GpsDesiredAccuracy));

        public Version GetSupportedQuestionnaireContentVersion()
        {
            return questionnaireContentVersionProvider.GetSupportedQuestionnaireContentVersion();
        }

        public string GetDeviceId()
        {
            return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
        }

        public string GetApplicationVersionName()
        {
            return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
        }

        public string GetDeviceTechnicalInformation()
        {
            var interviewIds = string.Join(","+ Environment.NewLine, this.interviewViewRepository.LoadAll().Select(i => i.InterviewId));
            var questionnaireIds = string.Join(","+ Environment.NewLine, this.questionnaireViewRepository.LoadAll().Select(i => i.Identity));

            return $"Version:{this.GetApplicationVersionName()} {Environment.NewLine}" +
                   $"SyncProtocolVersion:{this.syncProtocolVersionProvider.GetProtocolVersion()} {Environment.NewLine}" +
                   $"User:{GetUserInformation()} {Environment.NewLine}" +
                   $"DeviceId:{this.GetDeviceId()} {Environment.NewLine}" +
                   $"RAM:{GetRAMInformation()}% {Environment.NewLine}" +
                   $"DBSize:{GetDataBaseSize()} {Environment.NewLine}" +
                   $"Endpoint:{this.Endpoint}{Environment.NewLine}" +
                   $"AcceptUnsignedSslCertificate:{this.AcceptUnsignedSslCertificate} {Environment.NewLine}" +
                   $"BufferSize:{this.BufferSize} {Environment.NewLine}" +
                   $"Timeout:{this.Timeout} {Environment.NewLine}" +
                   $"CurrentDataTime:{DateTime.Now} {Environment.NewLine}" +
                   $"QuestionnairesList:{questionnaireIds} {Environment.NewLine}" + 
                   $"EventChunkSize:{this.EventChunkSize} {Environment.NewLine}" +
                   $"VibrateOnError:{this.VibrateOnError} {Environment.NewLine}" +
                   $"InterviewsList:{interviewIds}";
        }

        public void SetEventChunkSize(int eventChunkSize)
        {
            this.SaveCurrentSettings(settings =>
            {
                settings.EventChunkSize = eventChunkSize;
            });
        }

        public int GetApplicationVersionCode()
        {
            return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionCode;
        }

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

        public string BackupFolder => backupFolder;

        public string RestoreFolder => restoreFolder;

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
        {
            ActivityManager activityManager = Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;
            if (activityManager == null)
                return "UNKNOWN";

            ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
            activityManager.GetMemoryInfo(mi);
            return
                $"{FileSizeUtils.SizeSuffix(mi.TotalMem)} total, avaliable {(int) (((double) (100*mi.AvailMem))/mi.TotalMem)}";
        }

        private string GetDataBaseSize()
        {
            return
                FileSizeUtils.SizeSuffix(this.fileSystemAccessor.GetDirectorySize(FileSystem.Current.LocalStorage.Path));
        }
    }
}
