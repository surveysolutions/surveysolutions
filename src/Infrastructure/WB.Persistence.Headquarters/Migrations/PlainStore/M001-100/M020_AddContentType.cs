using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(20)]
    public class M020_AddContentType : Migration
    {
        public override void Up()
        {
            Create.Column("contenttype").OnTable("audiofiles").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("contenttype").FromTable("audiofiles");
        }
    }
}