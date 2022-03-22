using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202203211532)]
    public class M202203211532_MapBrowseItems_AddGeoJson : AutoReversingMigration
    {
        public override void Up()
        {
            this.Create
                .Column("geojson")
                .OnTable("mapbrowseitems")
                .AsString().Nullable();
            this.Create
                .Column("ispreviewgeojson")
                .OnTable("mapbrowseitems")
                .AsBoolean().NotNullable().WithDefaultValue(false);
        }
    }
}