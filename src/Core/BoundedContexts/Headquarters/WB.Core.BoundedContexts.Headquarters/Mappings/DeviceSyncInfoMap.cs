using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class DeviceSyncInfoMap : ClassMapping<DeviceSyncInfo>
    {
        public DeviceSyncInfoMap()
        {
            Id(x => x.Id, id =>
            {
                id.Generator(Generators.Identity);
                id.Column("\"Id\"");
            });
            Table("devicesyncinfo");

            Property(x => x.SyncDate);
            Property(x => x.InterviewerId);
            Property(x => x.DeviceId);
            Property(x => x.DeviceModel);
            Property(x => x.DeviceType);
            Property(x => x.DeviceDate);
            Property(x => x.DeviceLocationLat);
            Property(x => x.DeviceLocationLong);
            Property(x => x.DeviceLanguage);
            Property(x => x.DeviceManufacturer);
            Property(x => x.DeviceBuildNumber);
            Property(x => x.DeviceSerialNumber);
            Property(x => x.AndroidVersion);
            Property(x => x.AndroidSdkVersion);
            Property(x => x.AndroidSdkVersionName);
            Property(x => x.AppVersion);
            Property(x => x.AppBuildVersion);
            Property(x => x.LastAppUpdatedDate);
            Property(x => x.NetworkType);
            Property(x => x.NetworkSubType);
            Property(x => x.MobileOperator);
            Property(x => x.AppOrientation);
            Property(x => x.BatteryPowerSource);
            Property(x => x.BatteryChargePercent);
            Property(x => x.IsPowerInSaveMode);
            Property(x => x.MobileSignalStrength);
            Property(x => x.StorageTotalInBytes);
            Property(x => x.StorageFreeInBytes);
            Property(x => x.RAMTotalInBytes);
            Property(x => x.RAMFreeInBytes);
            Property(x => x.DBSizeInfo);
            Property(x => x.NumberOfStartedInterviews);

            ManyToOne(x => x.Statistics, mto =>
            {
                mto.Column("\"StatisticsId\"");
                mto.Cascade(Cascade.All);
            });
        }
    }

    public class SyncStatisticsMap : ClassMapping<SyncStatistics>
    {
        public SyncStatisticsMap()
        {
            Table("devicesyncstatistics");
            Id(x => x.Id, idmap =>
            {
                idmap.Column("\"Id\"");
                idmap.Generator(Generators.Identity);
            });

            Property(x => x.UploadedInterviewsCount);
            Property(x => x.DownloadedInterviewsCount);
            Property(x => x.DownloadedQuestionnairesCount);
            Property(x => x.RejectedInterviewsOnDeviceCount);
            Property(x => x.NewInterviewsOnDeviceCount);
            Property(x => x.NewAssignmentsCount);
            Property(x => x.RemovedAssignmentsCount);
            Property(x => x.RemovedInterviewsCount);
            Property(x => x.AssignmentsOnDeviceCount);
            Property(x => x.TotalUploadedBytes);
            Property(x => x.TotalDownloadedBytes);
            Property(x => x.TotalConnectionSpeed);
            Property(x => x.TotalSyncDuration, m =>
            {
                m.Type<TimeSpanType>();
            });
            Property(x => x.SyncFinishDate);
        }
    }
}
