using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WB.Core.BoundedContexts.Headquarters.Views.Device
{   
    public class DeviceSyncInfo
    {
        public virtual int Id { get; set; }
        public virtual DateTime SyncDate { get; set; }
        [Index("devicesyncinfo_interviewerid_androidsdkversion_appbuildversion", Order = 1)]
        public virtual Guid InterviewerId { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual string DeviceModel { get; set; }
        public virtual string DeviceType { get; set; }
        public virtual DateTime DeviceDate { get; set; }
        public virtual double? DeviceLocationLat { get; set; }
        public virtual double? DeviceLocationLong { get; set; }
        public virtual string DeviceLanguage { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceBuildNumber { get; set; }
        public string DeviceSerialNumber { get; set; }
        public virtual string AndroidVersion { get; set; }
        [Index("devicesyncinfo_interviewerid_androidsdkversion_appbuildversion", Order = 2)]
        public virtual int AndroidSdkVersion { get; set; }
        public string AndroidSdkVersionName { get; set; }
        public virtual string AppVersion { get; set; }
        [Index("devicesyncinfo_interviewerid_androidsdkversion_appbuildversion", Order = 3)]
        public virtual int AppBuildVersion { get; set; }
        public virtual DateTime LastAppUpdatedDate { get; set; }
        public virtual string NetworkType { get; set; }
        public virtual string NetworkSubType { get; set; }
        public virtual string MobileOperator { get; set; }
        public virtual string AppOrientation { get; set; }
        public virtual string BatteryPowerSource { get; set; }
        public virtual int BatteryChargePercent { get; set; }
        public bool IsPowerInSaveMode { get; set; }
        public virtual int MobileSignalStrength { get; set; }
        public virtual long StorageTotalInBytes { get; set; }
        public virtual long StorageFreeInBytes { get; set; }
        public virtual long RAMTotalInBytes { get; set; }
        public virtual long RAMFreeInBytes { get; set; }
        public virtual long DBSizeInfo { get; set; }
        public virtual int NumberOfStartedInterviews { get; set; }

        public virtual int? StatisticsId { get; set; }
        [ForeignKey(nameof(StatisticsId))]
        public virtual SyncStatistics Statistics { get; set; } 
    }

    public class SyncStatistics
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        public int UploadedInterviewsCount { get; set; }
        public int DownloadedInterviewsCount { get; set; }
        public int DownloadedQuestionnairesCount { get; set; }

        public int RejectedInterviewsOnDeviceCount { get; set; }
        public int NewInterviewsOnDeviceCount { get; set; }

        public int NewAssignmentsCount { get; set; }
        public int RemovedAssignmentsCount { get; set; }
        public int AssignmentsOnDeviceCount { get; set; }

        [DefaultValue(0)]
        public long TotalUploadedBytes { get; set; }

        [DefaultValue(0)]
        public long TotalDownloadedBytes { get; set; }

        /// <summary>
        /// Upload/Download summary connection speed in bytes/second
        /// </summary>
        [DefaultValue(0)]
        public double TotalConnectionSpeed { get; set; }

        public TimeSpan TotalSyncDuration { get; set; }

        public virtual DateTime SyncFinishDate { get; set; }
    }
}