using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Net;
using Android.OS;
using Android.Telephony;
using Java.Util;
using Plugin.DeviceInfo;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Settings;
using Permission = Plugin.Permissions.Abstractions.Permission;

namespace WB.UI.Shared.Enumerator.Services
{
    public class DeviceInformationService : IDeviceInformationService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IDeviceOrientation deviceOrientation;
        private readonly IBattery battery;
        private readonly IPermissions permissions;
        private readonly ILogger logger;
        private GsmSignalStrengthListener gsmSignalStrengthListener;

        public DeviceInformationService(IFileSystemAccessor fileSystemAccessor,
            IDeviceOrientation deviceOrientation,
            IBattery battery,
            IPermissions permissions,
            ILogger logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.deviceOrientation = deviceOrientation;
            this.battery = battery;
            this.permissions = permissions;
            this.logger = logger;

            //this.TryToCreateGsmSignalStrengthListener();
        }

        private void TryToCreateGsmSignalStrengthListener()
        {
            try
            {
                this.gsmSignalStrengthListener = new GsmSignalStrengthListener(this.telephonyManager);
            }
            catch (Exception e)
            {
                this.logger.Error("Could not create GsmSignalStrengthListener", e);
            }
        }

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
        
        public async Task<DeviceInfo> GetDeviceInfoAsync() => new DeviceInfo
        {
            DeviceId = this.TryGetDeviceId(),
            DeviceModel = this.TryGetDeviceModel(),
            DeviceType = this.TryGetDeviceType(),
            DeviceDate = DateTime.UtcNow,
            DeviceLanguage = this.TryGetDeviceLanguage(),
            DeviceLocation = await this.TryGetDeviceLocation().ConfigureAwait(false),
            DeviceManufacturer = this.TryGetDeviceManufacturer(),
            DeviceBuildNumber = this.TryGetDeviceBuildNumber(),
            DeviceSerialNumber = this.TryGetDeviceSerialNumber(),
            AndroidVersion = this.TryGetAndroidVersion(),
            AndroidSdkVersion = this.TryGetAndroidSdkVersion(),
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
                return CrossDeviceInfo.Current.Idiom.ToString();
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
                return this.gsmSignalStrengthListener?.SignalStrength ?? 0;
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
                return this.appPackageInfo.VersionCode;
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
                return this.appPackageInfo.VersionName;
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
                return (int)Build.VERSION.SdkInt;
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
                return Build.VERSION.Release;
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
                return CrossDeviceInfo.Current.Model;
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
                return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                    Android.Provider.Settings.Secure.AndroidId);
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
            this.activityManager.GetMemoryInfo(mi);

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
            //this.gsmSignalStrengthListener?.Dispose();
        }
    }
}
