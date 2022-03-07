using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202203051532)]
    public class M202203051532_MapBrowseItems_AddShapesColumns : AutoReversingMigration
    {
        public override void Up()
        {
            this.Create
                .Column("uploadedby")
                .OnTable("mapbrowseitems")
                .AsGuid().Nullable();
            this.Create
                .Column("shapescount")
                .OnTable("mapbrowseitems")
                .AsInt32().Nullable();
            this.Create
                .Column("shapetype")
                .OnTable("mapbrowseitems")
                .AsString().Nullable();
        }
    }
}