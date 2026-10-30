using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202610151800)]
    public class M202610151800_MapStorageGenerations : Migration
    {
        public override void Up()
        {
            Alter.Table("mapbrowseitems")
                .AddColumn("storagekey").AsString(255).Nullable();
            Alter.Table("mapfiledeletionrequests")
                .AddColumn("storagekey").AsString(255).Nullable();
        }

        public override void Down()
        {
            Delete.Column("storagekey").FromTable("mapfiledeletionrequests");
            Delete.Column("storagekey").FromTable("mapbrowseitems");
        }
    }
}
