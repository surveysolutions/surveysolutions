using System.ComponentModel;
using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201710161320)]
    public class M201710161320_UserMaps : Migration
    {
        public override void Up()
        {
            Create.Table("usermaps")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("map").AsString(255).NotNullable()
                .WithColumn("username").AsString(255).NotNullable();

            Create.Index("usermaps_map")
                .OnTable("usermaps")
                .OnColumn("map");

            Create.Index("usermaps_user")
                .OnTable("usermaps")
                .OnColumn("username");

            Create.ForeignKey("mapbrowseitems_mapbrowseitems")
                .FromTable("usermaps").ForeignColumn("map")
                .ToTable("mapbrowseitems").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
        }
    }
}