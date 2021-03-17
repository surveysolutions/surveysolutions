using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(202101281413)]
    public class M202101281413_Users_AddPasswordChangeRequiredColumn : Migration
    {
        public override void Up()
        {
            this.Create.Column("password_change_required")
                    .OnTable("users")
                    .AsBoolean()
                    .WithDefaultValue(false)
                    .NotNullable();
        }

        public override void Down()
        {
            Delete.Column("password_change_required").FromTable("users");
        }
    }
}
