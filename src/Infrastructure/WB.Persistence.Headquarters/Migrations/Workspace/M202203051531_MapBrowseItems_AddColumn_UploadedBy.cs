using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202203051531)]
    public class M202203051531_MapBrowseItems_AddColumn_UploadedBy : AutoReversingMigration
    {
        public override void Up()
        {
            this.Create
                .Column("uploadedby")
                .OnTable("mapbrowseitems")
                .AsGuid().Nullable();
        }
    }
}