using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201804031536)]
    public class M201804031536_MakeUserPreloadingOptionalFields : Migration
    {
        public override void Up()
        {
            Alter.Column("email").OnTable("usertoimport").AsString().Nullable();
            Alter.Column("fullname").OnTable("usertoimport").AsString().Nullable();
            Alter.Column("phonenumber").OnTable("usertoimport").AsString().Nullable();
            Alter.Column("supervisor").OnTable("usertoimport").AsString().Nullable();
        }

        public override void Down()
        {
        }
    }
}
