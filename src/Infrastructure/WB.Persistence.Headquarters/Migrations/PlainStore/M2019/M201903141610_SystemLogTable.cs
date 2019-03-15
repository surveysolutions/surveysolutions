using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201903141610)]
    public class M201903141610_SystemLogTable : Migration
    {
        public override void Up()
        {
            Create.Table("systemlog")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("type").AsInt32()
                .WithColumn("logdate").AsDateTime()
                .WithColumn("userid").AsGuid().Nullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("log").AsString();

            Create.Index("systemlog_userid")
                .OnTable("systemlog")
                .OnColumn("userid");
        }

        public override void Down()
        {
            Delete.Table("systemlog");
        }
    }
}
