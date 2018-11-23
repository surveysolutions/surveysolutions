using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201804231416)]
    public class M201804231416_AddAuditLogTable : Migration
    {
        public override void Up()
        {
            Create.Table("auditlogrecords")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("recordid").AsInt32()
                .WithColumn("responsibleid").AsGuid().Nullable()
                .WithColumn("responsiblename").AsString()
                .WithColumn("type").AsString()
                .WithColumn("time").AsDateTime()
                .WithColumn("timeutc").AsDateTime()
                .WithColumn("payload").AsString();

            Create.Index("auditlogrecords_responsibleid")
                .OnTable("auditlogrecords")
                .OnColumn("responsibleid");
            Create.Index("auditlogrecords_recordid")
                .OnTable("auditlogrecords")
                .OnColumn("recordid");
        }

        public override void Down()
        {
            Delete.Table("auditlogrecords");
        }
    }
}
