using System;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class DeviceInfoApiView
    {
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceType { get; set; }
        public DateTime DeviceDate { get; set; }
        public LocationAddressApiView DeviceLocation { get; set; }
        public string DeviceLanguage { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceBuildNumber { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string AndroidVersion { get; set; }
        public int AndroidSdkVersion { get; set; }
        public string AndroidSdkVersionName { get; set; }
        public string AppVersion { get; set; }
        public virtual int AppBuildVersion { get; set; }
        public DateTime LastAppUpdatedDate { get; set; }
        public string NetworkType { get; set; }
        public string NetworkSubType { get; set; }
        public string MobileOperator { get; set; }
        public string AppOrientation { get; set; }
        public string BatteryPowerSource { get; set; }
        public int BatteryChargePercent { get; set; }
        public bool IsPowerInSaveMode { get; set; }
        public int MobileSignalStrength { get; set; }
        public StorageInfoApiView StorageInfo { get; set; }
        public RAMInfoApiView RAMInfo { get; set; }
        public long DBSizeInfo { get; set; }
        public int NumberOfStartedInterviews { get; set; }

        public DateTimeOffset? ReceivedDate { get; set; }

        public Guid? UserId { get; set; }
    }

    public class RAMInfoApiView
    {
        public long Total { get; set; }
        public long Free { get; set; }
    }

    public class StorageInfoApiView
    {
        public long Total { get; set; }
        public long Free { get; set; }
    }

    public class LocationAddressApiView
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class UnexpectedExceptionApiView
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
