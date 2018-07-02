using System;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceType { get; set; }
        public DateTime DeviceDate { get; set; }
        public LocationAddress DeviceLocation { get; set; }
        public string DeviceLanguage { get; set; }
        public string AndroidVersion { get; set; }
        public int AndroidSdkVersion { get; set; }
        public string AndroidSdkVersionName { get; set; }
        public string AppVersion { get; set; }
        public int AppBuildVersion { get; set; }
        public DateTime LastAppUpdatedDate { get; set; }
        public string NetworkType { get; set; }
        public string NetworkSubType { get; set; }
        public string MobileOperator { get; set; }
        public string AppOrientation { get; set; }
        public string BatteryPowerSource { get; set; }
        public int BatteryChargePercent { get; set; }
        public bool IsPowerInSaveMode { get; set; }
        public int MobileSignalStrength { get; set; }
        public StorageInfo StorageInfo { get; set; }
        public RAMInfo RAMInfo { get; set; }
        public long DBSizeInfo { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceBuildNumber { get; set; }
        public string DeviceSerialNumber { get; set; }
    }

    public class RAMInfo
    {
        public long Total { get; set; }
        public long Free { get; set; }
    }

    public class StorageInfo
    {
        public long Total { get; set; }
        public long Free { get; set; }
    }
}
