using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(202005111654)]
    public class M202005111654_AddExternalUserName : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("usertoimport").Column("externalusername").Exists())
            {
                Create.Column("externalusername").OnTable("usertoimport").AsString().Nullable();
            }
        }

        public override void Down()
        {
            Delete.Column("externalusername").FromTable("usertoimport");
        }
    }
}
