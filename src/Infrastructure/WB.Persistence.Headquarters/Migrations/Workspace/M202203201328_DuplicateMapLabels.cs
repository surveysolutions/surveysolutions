using System.ComponentModel;
using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202203201328)]
    public class M202203201328_DuplicateMapLabels : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("duplicatemaplabels")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("map").AsString(255).NotNullable()
                .WithColumn("label").AsString().NotNullable()
                .WithColumn("count").AsInt32().NotNullable();

            Create.Index("duplicatemaplabels_map")
                .OnTable("duplicatemaplabels")
                .OnColumn("map");

            Create.ForeignKey("duplicatemaplabels_mapbrowseitems")
                .FromTable("duplicatemaplabels").ForeignColumn("map")
                .ToTable("mapbrowseitems").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }
    }
}