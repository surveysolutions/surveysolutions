using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(202103191418)]
    public class M202103191418_AddGlobalAuditLogTable : Migration
    {
        public override void Up()
        {
            Create.Table("auditlogrecords")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("record_id").AsInt32()
                .WithColumn("responsible_id").AsGuid().Nullable()
                .WithColumn("responsible_name").AsString().Nullable()
                .WithColumn("type").AsString()
                .WithColumn("time").AsDateTime()
                .WithColumn("time_utc").AsDateTime()
                .WithColumn("payload").AsString();

            Create.Index("auditlogrecords_responsible_id")
                .OnTable("auditlogrecords")
                .OnColumn("responsibleid");
            Create.Index("auditlogrecords_record_id")
                .OnTable("auditlogrecords")
                .OnColumn("recordid");
        }

        public override void Down()
        {
            Delete.Table("auditlogrecords");
        }
    }
}
