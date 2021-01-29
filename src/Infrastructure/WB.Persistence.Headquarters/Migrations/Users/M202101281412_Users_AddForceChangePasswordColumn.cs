using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(202101281412)]
    public class M202101281412_Users_AddForceChangePasswordColumn : Migration
    {
        public override void Up()
        {
            this.Create.Column("ForceChangePassword")
                    .OnTable("users")
                    .AsBoolean()
                    .WithDefaultValue(false)
                    .NotNullable();
        }

        public override void Down()
        {
            Delete.Column("ForceChangePassword").FromTable("users");
        }
    }
}
