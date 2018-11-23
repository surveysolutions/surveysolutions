using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201711141615)]
    public class M201711141615_ImportUsers : Migration
    {
        public override void Up()
        {
            Delete.Table("userpreloadingverificationerrors");
            Delete.Table("userprelodingdata");
            Delete.Table("userpreloadingprocesses");

            Create.Table("usertoimport")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("login").AsString().NotNullable()
                .WithColumn("email").AsString().NotNullable()
                .WithColumn("fullname").AsString().NotNullable()
                .WithColumn("password").AsString().NotNullable()
                .WithColumn("phonenumber").AsString().NotNullable()
                .WithColumn("role").AsString().NotNullable()
                .WithColumn("supervisor").AsString().NotNullable();

            Create.Table("usersimportprocess")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("supervisorscount").AsInt32().NotNullable()
                .WithColumn("interviewerscount").AsInt32().NotNullable()
                .WithColumn("responsible").AsString().NotNullable()
                .WithColumn("starteddate").AsDateTime().NotNullable();
        }

        public override void Down()
        {
        }
    }
}