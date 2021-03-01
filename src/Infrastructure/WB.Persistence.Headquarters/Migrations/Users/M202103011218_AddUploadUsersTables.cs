using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(202103011218)]
    public class M202103011218_AddUploadUsersTables : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("usertoimport")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("login").AsString().NotNullable()
                .WithColumn("email").AsString().NotNullable()
                .WithColumn("fullname").AsString().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("phonenumber").AsString().Nullable()
                .WithColumn("role").AsString().NotNullable()
                .WithColumn("supervisor").AsString().Nullable()
                .WithColumn("workspace").AsString().Nullable();

            Create.Table("usersimportprocess")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("supervisorscount").AsInt32().NotNullable()
                .WithColumn("interviewersusersimportprocess count").AsInt32().NotNullable()
                .WithColumn("responsible").AsString().NotNullable()
                .WithColumn("starteddate").AsDateTime().NotNullable()
                .WithColumn("workspace").AsString().NotNullable();
        }
    }
}
