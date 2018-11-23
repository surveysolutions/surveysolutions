using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(5)]
    [Localizable(false)]
    public class M005_AddDeviceRegistrationDateToProfile : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("userprofiles").Column("DeviceRegistrationDate").Exists())
            {
                this.Alter.Table("userprofiles").AddColumn("DeviceRegistrationDate").AsDate().Nullable();
            }

            Execute.Sql($@"UPDATE users.userprofiles
                SET (""DeviceRegistrationDate"") = (
                    SELECT registrationdate
                    FROM readside.tabletdocuments
                    WHERE readside.tabletdocuments.androidid = users.userprofiles.""DeviceId"")");

            Execute.Sql("DROP TABLE readside.tabletdocuments");
        }

        public override void Down()
        {
            this.Delete.Column("DeviceRegistrationDate").FromTable("userprofiles");
        }
    }
}