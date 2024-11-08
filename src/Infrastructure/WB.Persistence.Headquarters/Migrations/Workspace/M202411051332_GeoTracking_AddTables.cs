using System.ComponentModel;
using System.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202411051332)]
    public class M202411051332_GeoTracking_AddTables : Migration // AutoReversingMigration
    {
        public override void Up()
        {
            if (Schema.Table("geo_tracking_points").Exists())
                Delete.Table("geo_tracking_points");
            if (Schema.Table("geo_tracking_records").Exists())
                Delete.Table("geo_tracking_records");
            
            Create.Table("geo_tracking_records")
                .WithColumn("id").AsInt64().Identity().PrimaryKey()
                .WithColumn("interviewer_id").AsGuid().NotNullable()
                .WithColumn("assignment_id").AsInt32().NotNullable()
                .WithColumn("start_date").AsString().NotNullable()
                .WithColumn("end_date").AsString().Nullable();
            Create.Index().OnTable("geo_tracking_records").OnColumn("interviewer_id");
            Create.Index().OnTable("geo_tracking_records").OnColumn("assignment_id");
            
            Create.ForeignKey("fk_geo_tracking_records__assignments")
                .FromTable("geo_tracking_records").ForeignColumn("assignment_id")
                .ToTable("assignments").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);

            
            Create.Table("geo_tracking_points")
                .WithColumn("position").AsInt32().NotNullable()
                .WithColumn("record_id").AsInt64().NotNullable()
                .WithColumn("longitude").AsDouble().NotNullable()
                .WithColumn("latitude").AsDouble().NotNullable()
                .WithColumn("time").AsString().NotNullable();
            Create.Index("idx_geo_tracking_points_record_id").OnTable("geo_tracking_points").OnColumn("record_id");

            Create.ForeignKey("fk_geo_tracking_records__geo_tracking_points")
                .FromTable("geo_tracking_points").ForeignColumn("record_id")
                .ToTable("geo_tracking_records").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.Table("geo_tracking_points");
            Delete.Table("geo_tracking_records");
        }
    }
}
