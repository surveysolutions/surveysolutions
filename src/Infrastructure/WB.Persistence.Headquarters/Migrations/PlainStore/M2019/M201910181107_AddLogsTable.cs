using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201910181107)]
    public class M201910181107_AddLogsTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("tablet_logs")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("device_id").AsString().Nullable()
                .WithColumn("user_name").AsString().Nullable()
                .WithColumn("content").AsBinary()
                .WithColumn("receive_date_utc").AsDateTime().NotNullable();
        }
    }
}
