using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201907241548)]
    public class M201907241548_RemoveUnusedMapsColumns : Migration
    {
        public override void Up()
        {
            Delete.Column("wkid").FromTable("mapbrowseitems");
            Delete.Column("xmaxval").FromTable("mapbrowseitems");
            Delete.Column("xminval").FromTable("mapbrowseitems");
            Delete.Column("ymaxval").FromTable("mapbrowseitems");
            Delete.Column("yminval").FromTable("mapbrowseitems");
            Delete.Column("maxscale").FromTable("mapbrowseitems");
            Delete.Column("minscale").FromTable("mapbrowseitems");
        }

        public override void Down()
        {
        }
    }
}
