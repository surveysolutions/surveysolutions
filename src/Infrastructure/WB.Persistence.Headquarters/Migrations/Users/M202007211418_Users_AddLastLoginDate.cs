using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(202007211418)]
    public class M202007211418_Users_AddLastLoginDate : Migration
    {
        public override void Up()
        {
            this.Create.Column("LastLoginDate")
                    .OnTable("users")
                    .AsDateTime()
                    .Nullable();
        }

        public override void Down()
        {
            Delete.Column("LastLoginDate").FromTable("users");
        }
    }
}
