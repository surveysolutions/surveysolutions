using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(22)]
    public class M022_AddIndexOnInterviewerIdFor_DeviceSyncInfo : Migration
    {
        public override void Up()
        {
            Create.Index("devicesyncinfo_interviewerid_androidsdkversion_appbuildversion")
                .OnTable("devicesyncinfo")
                .OnColumn("InterviewerId").Ascending()
                .OnColumn("AppBuildVersion").Ascending()
                .OnColumn("AndroidSdkVersion").Ascending();
        }

        public override void Down()
        {
            Delete.Index("devicesyncinfo_interviewerid_androidsdkversion_appbuildversion");
        }
    }
}