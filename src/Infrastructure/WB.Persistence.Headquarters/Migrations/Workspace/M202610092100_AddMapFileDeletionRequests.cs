using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202610092100)]
    public class M202610092100_AddMapFileDeletionRequests : Migration
    {
        public override void Up()
        {
            Create.Table("mapfiledeletionrequests")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("filename").AsString(255).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("mapfiledeletionrequests");
        }
    }
}
