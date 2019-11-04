using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201911041037)]
    public class M201911041037_AddDeviceSyncInfo : Migration
    {
        public override void Up()
        {
            if (!Schema.Schema("plainstore").Table("devicesyncstatistics").Exists())
            {
                this.Create.Table("devicesyncstatistics")
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("UploadedInterviewsCount").AsInt32().NotNullable()
                    .WithColumn("DownloadedInterviewsCount").AsInt32().NotNullable()
                    .WithColumn("DownloadedQuestionnairesCount").AsInt32().NotNullable()
                    .WithColumn("RejectedInterviewsOnDeviceCount").AsInt32().NotNullable()
                    .WithColumn("NewInterviewsOnDeviceCount").AsInt32().NotNullable()
                    .WithColumn("NewAssignmentsCount").AsInt32().NotNullable()
                    .WithColumn("RemovedAssignmentsCount").AsInt32().NotNullable()
                    .WithColumn("RemovedInterviewsCount").AsInt32()
                    .WithColumn("AssignmentsOnDeviceCount").AsInt32().NotNullable()
                    .WithColumn("TotalUploadedBytes").AsInt64().NotNullable()
                    .WithColumn("TotalDownloadedBytes").AsInt64().NotNullable()
                    .WithColumn("TotalConnectionSpeed").AsDouble().NotNullable()
                    .WithColumn("TotalSyncDuration").AsCustom("interval(6)").NotNullable()
                    .WithColumn("SyncFinishDate").AsDateTime().NotNullable();
            }

            if (!Schema.Schema("plainstore").Table("devicesyncinfo").Exists())
            {
                this.Create.Table("devicesyncinfo")
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("SyncDate").AsDateTime().NotNullable()
                    .WithColumn("InterviewerId").AsGuid().NotNullable()
                    .WithColumn("DeviceId").AsString()
                    .WithColumn("DeviceModel").AsString()
                    .WithColumn("DeviceType").AsString()
                    .WithColumn("DeviceDate").AsDateTime().NotNullable()
                    .WithColumn("DeviceLocationLat").AsDouble()
                    .WithColumn("DeviceLocationLong").AsDouble()
                    .WithColumn("DeviceLanguage").AsString()
                    .WithColumn("DeviceManufacturer").AsString()
                    .WithColumn("DeviceBuildNumber").AsString()
                    .WithColumn("DeviceSerialNumber").AsString()
                    .WithColumn("AndroidVersion").AsString()
                    .WithColumn("AndroidSdkVersion").AsInt32().NotNullable()
                    .WithColumn("AndroidSdkVersionName").AsString()
                    .WithColumn("AppVersion").AsString()
                    .WithColumn("AppBuildVersion").AsInt32().NotNullable()
                    .WithColumn("LastAppUpdatedDate").AsDateTime().NotNullable()
                    .WithColumn("NetworkType").AsString()
                    .WithColumn("NetworkSubType").AsString()
                    .WithColumn("MobileOperator").AsString()
                    .WithColumn("AppOrientation").AsString()
                    .WithColumn("BatteryPowerSource").AsString()
                    .WithColumn("BatteryChargePercent").AsInt32().NotNullable()
                    .WithColumn("IsPowerInSaveMode").AsBoolean().NotNullable()
                    .WithColumn("MobileSignalStrength").AsInt32().NotNullable()
                    .WithColumn("StorageTotalInBytes").AsInt64().NotNullable()
                    .WithColumn("StorageFreeInBytes").AsInt64().NotNullable()
                    .WithColumn("RAMTotalInBytes").AsInt64().NotNullable()
                    .WithColumn("RAMFreeInBytes").AsInt64().NotNullable()
                    .WithColumn("DBSizeInfo").AsInt64().NotNullable()
                    .WithColumn("NumberOfStartedInterviews").AsInt32().NotNullable()
                    .WithColumn("StatisticsId").AsInt32();

                this.Create.ForeignKey() 
                    .FromTable("devicesyncinfo").ForeignColumn("StatisticsId")
                    .ToTable("devicesyncstatistics").PrimaryColumn("Id");
            }
        }

        public override void Down()
        {
            this.Delete.Table("devicesyncinfo");
            this.Delete.Table("devicesyncstatistics");
        }
    }
}
