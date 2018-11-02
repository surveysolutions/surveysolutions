using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(3)]
    [Localizable(false)]
    public class M003_AddDeviceAppVersionToProfile : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("userprofiles").Column("DeviceAppVersion").Exists())
            {
                this.Alter.Table("userprofiles")
                    .AddColumn("DeviceAppVersion").AsString().Nullable()
                    .AddColumn("DeviceAppBuildVersion").AsInt32().Nullable();
            }

            if (Schema.Schema("plainstore").Table("devicesyncinfo").Exists())
            {

                this.Execute.Sql(@"DO $$
                DECLARE 
	                rec RECORD;
	                profileId integer;
                BEGIN

                FOR rec IN 
	                SELECT DISTINCT ON (""InterviewerId"") ""InterviewerId"", ""AppVersion"", ""AppBuildVersion""
	                FROM plainstore.devicesyncinfo
	                ORDER BY ""InterviewerId"", ""Id"" DESC
                LOOP
	                SELECT p.""Id"" FROM users.userprofiles p
	                JOIN users.users as u ON ""UserProfileId"" = p.""Id""
	                WHERE rec.""InterviewerId"" = u.""Id""
	                INTO profileId;

	                IF profileid IS NULL THEN
		                INSERT INTO users.userprofiles(""DeviceAppVersion"", ""DeviceAppBuildVersion"")
 		                VALUES (NULL, NULL)
		                RETURNING ""Id"" INTO profileid;
	                END IF;

	                RAISE NOTICE 'Rec: InterviewerId: (%), AppVersion: {%}, AppBuildVersion: {%}', rec.""InterviewerId"", rec.""AppVersion"", rec.""AppBuildVersion"";
	                RAISE NOTICE 'Profile id (%)', profileId;
	
	                UPDATE users.userprofiles
	                SET ""DeviceAppVersion"" = rec.""AppVersion"", ""DeviceAppBuildVersion"" = rec.""AppBuildVersion""
	                WHERE ""Id"" = profileId;

                END LOOP;	
                END
                $$");
            }
        }

        public override void Down()
        {
            this.Delete.Column("DeviceAppVersion").FromTable("userprofiles");
            this.Delete.Column("DeviceAppBuildVersion").FromTable("userprofiles");
        }
    }
}
