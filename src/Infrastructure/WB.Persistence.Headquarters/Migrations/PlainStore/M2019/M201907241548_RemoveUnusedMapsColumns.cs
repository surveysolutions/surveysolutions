using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201907241548)]
    public class M201907241548_RemoveUnusedMapsColumns : Migration
    {
        public override void Up()
        {
            Delete.Column("wkid")
                .Column("xmaxval")
                .Column("xminval")
                .Column("ymaxval")
                .Column("yminval")
                .Column("maxscale")
                .Column("minscale")
                .FromTable("mapbrowseitems");
        }

        public override void Down()
        {
        }
    }
}
