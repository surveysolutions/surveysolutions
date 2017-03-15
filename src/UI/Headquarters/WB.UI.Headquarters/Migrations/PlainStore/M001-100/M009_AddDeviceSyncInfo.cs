using FluentMigrator;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(9)]
    public class M009_AddDeviceSyncInfo : Migration
    {
        public override void Up()
        {
            Create.Table(@"devicesyncinfo")
                .WithColumn(nameof(DeviceSyncInfo.Id).ToLower()).AsInt32().PrimaryKey()
                .WithColumn(nameof(DeviceSyncInfo.InterviewerId).ToLower()).AsGuid()
                .WithColumn(nameof(DeviceSyncInfo.DeviceId).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.NumberOfStartedInterviews).ToLower()).AsInt32()
                .WithColumn(nameof(DeviceSyncInfo.NetworkSubType).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.NetworkType).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.DeviceLanguage).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.DeviceModel).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.DeviceType).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.DeviceDate).ToLower()).AsDateTime()
                .WithColumn(nameof(DeviceSyncInfo.AndroidSdkVersion).ToLower()).AsInt32()
                .WithColumn(nameof(DeviceSyncInfo.AndroidVersion).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.AppOrientation).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.AppVersion).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.BatteryChargePercent).ToLower()).AsInt32()
                .WithColumn(nameof(DeviceSyncInfo.BatteryPowerSource).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.DBSizeInfo).ToLower()).AsInt64()
                .WithColumn(nameof(DeviceSyncInfo.DeviceLocationLat).ToLower()).AsDouble().Nullable()
                .WithColumn(nameof(DeviceSyncInfo.DeviceLocationLong).ToLower()).AsDouble().Nullable()
                .WithColumn(nameof(DeviceSyncInfo.LastAppUpdatedDate).ToLower()).AsDateTime()
                .WithColumn(nameof(DeviceSyncInfo.MobileOperator).ToLower()).AsString()
                .WithColumn(nameof(DeviceSyncInfo.MobileSignalStrength).ToLower()).AsInt32()
                .WithColumn(nameof(DeviceSyncInfo.RAMFreeInBytes).ToLower()).AsInt64().Nullable()
                .WithColumn(nameof(DeviceSyncInfo.RAMTotalInBytes).ToLower()).AsInt64().Nullable()
                .WithColumn(nameof(DeviceSyncInfo.StorageFreeInBytes).ToLower()).AsInt64().Nullable()
                .WithColumn(nameof(DeviceSyncInfo.StorageTotalInBytes).ToLower()).AsInt64().Nullable();
        }

        public override void Down()
        {
            Delete.Table(@"devicesyncinfo");
        }
    }
}