using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Net;
using Android.OS;
using Android.Telephony;
using Android.Telephony.Gsm;
using Android.Views;
using Plugin.DeviceInfo;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Utils;
using Environment = System.Environment;
using Java.Util;
using Plugin.Permissions.Abstractions;
using Permission = Plugin.Permissions.Abstractions.Permission;

namespace WB.UI.Interviewer.Settings
{
    internal class InterviewerSettings : IInterviewerSettings, IDisposable
    {
        private readonly IPlainStorage<ApplicationSettingsView> settingsStorage;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;
        private readonly IQuestionnaireContentVersionProvider questionnaireContentVersionProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IDeviceOrientation deviceOrientation;
        private readonly IBattery battery;
        private readonly IPermissions permissions;
        private readonly GsmSignalStrengthListener gsmSignalStrengthListener;

        private PackageInfo appPackageInfo =>
            Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.MetaData);

        private ActivityManager activityManager
            => Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;

        private ConnectivityManager connectivityManager
            => Application.Context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

        private TelephonyManager telephonyManager
            => Application.Context.GetSystemService(Context.TelephonyService) as TelephonyManager;

        private LocationManager locationManager
            => Application.Context.GetSystemService(Context.LocationService) as LocationManager;

        private PowerManager powerManager => Application.Context.GetSystemService(Context.PowerService) as PowerManager;

        public InterviewerSettings(
            IPlainStorage<ApplicationSettingsView> settingsStorage, 
            ISyncProtocolVersionProvider syncProtocolVersionProvider, 
            IQuestionnaireContentVersionProvider questionnaireContentVersionProvider,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            IPlainStorage<InterviewView> interviewViewRepository, 
            IPlainStorage<QuestionnaireView> questionnaireViewRepository, 
            IFileSystemAccessor fileSystemAccessor,
            IDeviceOrientation deviceOrientation,
            IBattery battery,
            IPermissions permissions,
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
            this.deviceOrientation = deviceOrientation;
            this.battery = battery;
            this.permissions = permissions;
            this.BackupFolder = backupFolder;
            this.RestoreFolder = restoreFolder;

            gsmSignalStrengthListener = new GsmSignalStrengthListener(this.telephonyManager);
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

        public async Task<DeviceInfo> GetDeviceInfoAsync() => new DeviceInfo
        {
            DeviceId = this.TryGetDeviceId(),
            DeviceModel = this.TryGetDeviceModel(),
            DeviceType = this.TryGetDeviceType(),
            DeviceDate = DateTime.Now,
            DeviceLanguage = this.TryGetDeviceLanguage(),
            DeviceLocation = await this.TryGetDeviceLocation().ConfigureAwait(false),
            DeviceManufacturer = this.TryGetDeviceManufacturer(),
            DeviceBuildNumber = this.TryGetDeviceBuildNumber(),
            DeviceSerialNumber = this.TryGetDeviceSerialNumber(),
            AndroidVersion = this.TryGetAndroidVersion(),
            AndroidSdkVersion = TryGetAndroidSdkVersion(),
            AndroidSdkVersionName = TryGetAndroidSdkVersionName(),
            AppVersion = this.TryGetApplicationVersionName(),
            AppBuildVersion = this.TryGetApplicationVersionCode(),
            LastAppUpdatedDate = this.TryGetLastAppUpdatedDate(),
            AppOrientation = this.TryGetAppOrientation(),
            BatteryChargePercent = this.TryGetRemainingChargePercent(),
            BatteryPowerSource = this.TryGetBatteryPowerSource(),
            IsPowerInSaveMode = this.TryGetIsPowerInSaveMode(),
            MobileOperator = this.TryGetMobileOperatorName(),
            MobileSignalStrength = this.TryGetMobileSignalStrength(),
            NetworkType = this.TryGetNetworkType(),
            NetworkSubType = this.TryGetNetworkSubType(),
            DBSizeInfo = this.TryGetDirectorySize(),
            RAMInfo = this.TryGetRamInfo(),
            StorageInfo = this.TryGetStorageInfo()
        };

        private string TryGetDeviceType()
        {
            try
            {
                return this.GetDeviceType();
            }
            catch
            {
                return string.Empty;
            }
        }

        private StorageInfo TryGetStorageInfo()
        {
            try
            {
                return this.GetStorageInfo();
            }
            catch
            {
                return null;
            }
        }

        private RAMInfo TryGetRamInfo()
        {
            try
            {
                return this.GetRAMInfo();
            }
            catch
            {
                return null;
            }
        }

        private long TryGetDirectorySize()
        {
            try
            {
                return this.fileSystemAccessor.GetDirectorySize(AndroidPathUtils.GetPathToInternalDirectory());
            }
            catch
            {
                return 0;
            }
        }

        private string TryGetNetworkSubType()
        {
            try
            {
                return this.connectivityManager?.ActiveNetworkInfo?.SubtypeName;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string TryGetNetworkType()
        {
            try
            {
                return this.connectivityManager?.ActiveNetworkInfo?.TypeName;
            }
            catch
            {
                return string.Empty;
            }
            
        }

        private int TryGetMobileSignalStrength()
        {
            try
            {
                return this.gsmSignalStrengthListener.SignalStrength;
            }
            catch
            {
                return 0;
            }
        }

        private string TryGetMobileOperatorName()
        {
            try
            {
                return this.telephonyManager?.NetworkOperatorName;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool TryGetIsPowerInSaveMode()
        {
            try
            {
                return Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && this.powerManager.IsPowerSaveMode;
            }
            catch
            {
                return false;
            }
        }

        private string TryGetBatteryPowerSource()
        {
            try
            {
                return this.battery.GetPowerSource().ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private int TryGetRemainingChargePercent()
        {
            try
            {
                return this.battery.GetRemainingChargePercent();
            }
            catch
            {
                return 0;
            }
        }

        private string TryGetAppOrientation()
        {
            try
            {
                return this.deviceOrientation.GetOrientation().ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private DateTime TryGetLastAppUpdatedDate()
        {
            try
            {
                return new DateTime(1970, 1, 1).AddMilliseconds(this.appPackageInfo.LastUpdateTime);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private int TryGetApplicationVersionCode()
        {
            try
            {
                return this.GetApplicationVersionCode();
            }
            catch
            {
                return 0;
            }
        }

        private string TryGetApplicationVersionName()
        {
            try
            {
                return this.GetApplicationVersionName();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string TryGetAndroidSdkVersionName()
        {
            try
            {
                return Build.VERSION.SdkInt.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private int TryGetAndroidSdkVersion()
        {
            try
            {
                return (int) Build.VERSION.SdkInt;
            }
            catch
            {
                return 0;
            }
        }

        private string TryGetAndroidVersion()
        {
            try
            {
                return this.GetAndroidVersion();
            }
            catch
            {
                return string.Empty;
            }
        }

        private string TryGetDeviceSerialNumber()
        {
            try
            {
                return Build.Serial;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string TryGetDeviceBuildNumber()
        {
            try
            {
                return Build.Display;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string TryGetDeviceManufacturer()
        {
            try
            {
                return Build.Manufacturer;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<LocationAddress> TryGetDeviceLocation()
        {
            try
            {
                return await this.GetDeviceLocationAsync();
            }
            catch
            {
                return await Task.FromResult<LocationAddress>(null);
            }
        }

        private string TryGetDeviceLanguage()
        {
            try
            {
                return Locale.Default.DisplayLanguage;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string TryGetDeviceModel()
        {
            try
            {
                return this.GetDeviceModel();
            }
            catch
            {
                return string.Empty;
            }
        }

        private string TryGetDeviceId()
        {
            try
            {
                return this.GetDeviceId();
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<LocationAddress> GetDeviceLocationAsync()
        {
            var locationPermissionsStatus = await this.permissions.CheckPermissionStatusAsync(Permission.Location);
            if (locationPermissionsStatus != PermissionStatus.Granted) return null;

            var locationProvider = this.locationManager.GetBestProvider(new Criteria
            {
                AltitudeRequired = false,
                PowerRequirement = Power.Low,
                SpeedRequired = false,
                Accuracy = Accuracy.Low,
                BearingRequired = false
            }, true);
            var location = this.locationManager.GetLastKnownLocation(locationProvider);

            return location == null ? null : new LocationAddress
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
        }

        private RAMInfo GetRAMInfo()
        {
            if (this.activityManager == null)
                return null;

            ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
            activityManager.GetMemoryInfo(mi);

            return new RAMInfo
            {
                Total = mi.TotalMem,
                Free = mi.AvailMem
            };
        }

        private StorageInfo GetStorageInfo()
        {
            StatFs stat = new StatFs(Android.OS.Environment.DataDirectory.Path);

            return new StorageInfo
            {
                Free = stat.AvailableBlocksLong * stat.BlockSizeLong,
                Total = stat.BlockCountLong * stat.BlockSizeLong
            };
        }

        public void Dispose()
        {
            this.gsmSignalStrengthListener.Dispose();
        }
    }
}
