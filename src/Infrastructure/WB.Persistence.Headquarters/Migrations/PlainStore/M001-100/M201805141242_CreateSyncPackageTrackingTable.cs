using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201805141242)]
    [Localizable(false)]
    public class M201805141242_CreateSyncPackageTrackingTable : Migration
    {
        public override void Up()
        {
            Create.Table("receivedpackagelogentries")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("firsteventid").AsGuid().NotNullable().Indexed()
                .WithColumn("lasteventid").AsGuid().NotNullable()
                .WithColumn("firsteventtimestamp").AsDateTime().NotNullable()
                .WithColumn("lasteventtimestamp").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("receivedpackagelogentries");
        }
    }
}
