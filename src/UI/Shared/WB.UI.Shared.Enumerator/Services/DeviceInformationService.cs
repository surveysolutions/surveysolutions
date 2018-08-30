using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Net;
using Android.OS;
using Android.Telephony;
using Java.Lang;
using Java.Util;
using Plugin.DeviceInfo;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Settings;
using Exception = System.Exception;
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

        /// <summary>
        /// Ported from java snippet https://gist.github.com/flawyte/efd23dd520fc2320f94ba003b9aabfce
        /// </summary>
        private string TryGetDeviceSerialNumber()
        {
            try
            {
                Java.Lang.String serialnum = null;
                var emptyJavaString = new Java.Lang.String(new StringBuffer(""));
                try
                {
                    var c = Class.ForName("android.os.SystemProperties");
                    var get = c.GetMethod("get", Class.FromType(typeof(Java.Lang.String)));

                    // (?) Lenovo Tab (https://stackoverflow.com/a/34819027/1276306)
                    serialnum = (Java.Lang.String)get.Invoke(c, "gsm.sn1");

                    if (serialnum.Equals(emptyJavaString))
                        // Samsung Galaxy S5 (SM-G900F) : 6.0.1
                        // Samsung Galaxy S6 (SM-G920F) : 7.0
                        // Samsung Galaxy Tab 4 (SM-T530) : 5.0.2
                        // (?) Samsung Galaxy Tab 2 (https://gist.github.com/jgold6/f46b1c049a1ee94fdb52)
                        serialnum = (Java.Lang.String)get.Invoke(c, "ril.serialnumber");

                    if (serialnum.Equals(emptyJavaString))
                        // Google Nexus 5 : 6.0.1
                        // Honor 5C (NEM-L51) : 7.0
                        // Honor 5X (KIW-L21) : 6.0.1
                        // Huawei M2 (M2-801w) : 5.1.1
                        // (?) HTC Nexus One : 2.3.4 (https://gist.github.com/tetsu-koba/992373)
                        serialnum = (Java.Lang.String)get.Invoke(c, "ro.serialno");

                    if (serialnum.Equals(emptyJavaString))
                        // (?) Samsung Galaxy Tab 3 (https://stackoverflow.com/a/27274950/1276306)
                        serialnum = (Java.Lang.String)get.Invoke(c, "sys.serialnumber");

                }
                catch (Java.Lang.Exception)
                {
                }

                string serial = string.Empty;
                if (serialnum == null || serialnum.Equals(emptyJavaString))
                {
                    try
                    {
                        serial = Build.Serial;
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    serial = serialnum.ToString();
                }

                return serial;
            }
            catch (Exception)
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
