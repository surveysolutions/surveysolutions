using FluentMigrator;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(202004151533)]
    public class M202004151533_TwoFactorAuth : Migration
    {
        public override void Up()
        {
            this.CreateTableIfNotExists("usertokens", t => t
                .WithColumn("UserId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey()
                .WithColumn("Name").AsString(128).NotNullable().PrimaryKey().Indexed()
                .WithColumn("Value").AsString().Nullable());
        }

        public override void Down()
        {
            Delete.Table("usertokens");
        }
    }
}
