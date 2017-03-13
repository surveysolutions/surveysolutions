using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class DeviceSyncInfoMap : ClassMapping<DeviceSyncInfo>
    {
        public DeviceSyncInfoMap()
        {
            Table("DeviceSyncInfo");
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));

            Property(x => x.InterviewerId);
            Property(x => x.DeviceId);
            Property(x => x.DeviceModel);
            Property(x => x.DeviceType);
            Property(x => x.DeviceLanguage);
            Property(x => x.DeviceDate);
            Property(x => x.DeviceLocationAddress);
            Property(x => x.DeviceLocationLat);
            Property(x => x.DeviceLocationLong);
            Property(x => x.AndroidSdkVersion);
            Property(x => x.AndroidVersion);
            Property(x => x.LastAppUpdatedDate);
            Property(x => x.AppVersion);
            Property(x => x.AppOrientation);
            Property(x => x.BatteryChargePercent);
            Property(x => x.BatteryPowerSource);
            Property(x => x.DBSizeInfo);
            Property(x => x.MobileOperator);
            Property(x => x.MobileSignalStrength);
            Property(x => x.NetworkType);
            Property(x => x.NetworkSubType);
            Property(x => x.NumberOfStartedInterviews);
            Property(x => x.RAMFreeInBytes);
            Property(x => x.RAMTotalInBytes);
            Property(x => x.StorageFreeInBytes);
            Property(x => x.StorageTotalInBytes);
        }
    }
}