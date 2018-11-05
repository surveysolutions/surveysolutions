using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201710051530)]
    public class M201710051530_MapBrowseItems : Migration
    {
        public override void Up()
        {
            Create.Table("mapbrowseitems")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("size").AsInt64().Nullable()
                .WithColumn("importdate").AsDateTime().Nullable()
                .WithColumn("filename").AsString().Nullable()
                .WithColumn("wkid").AsInt64().Nullable()
                .WithColumn("xmaxval").AsDouble().Nullable()
                .WithColumn("xminval").AsDouble().Nullable()
                .WithColumn("ymaxval").AsDouble().Nullable()
                .WithColumn("yminval").AsDouble().Nullable()
                .WithColumn("maxscale").AsDouble().Nullable()
                .WithColumn("minscale").AsDouble().Nullable();
        }

        public override void Down()
        {
        }
    }
}