using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(25)]
    [Localizable(false)]
    public class M025_RemovedUserDocumentAndRolesTables : Migration
    {
        public override void Up()
        {
            Delete.Table("userdocuments");
            Delete.Table("roles");
        }

        public override void Down()
        {
            Create.Table("userdocuments")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("creationdate").AsDateTime().Nullable()
                .WithColumn("email").AsString().Nullable()
                .WithColumn("islockedbyhq").AsBoolean().Nullable()
                .WithColumn("isarchived").AsBoolean().Nullable()
                .WithColumn("islockedbysupervisor").AsBoolean().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("publickey").AsGuid().Nullable()
                .WithColumn("supervisorid").AsGuid().Nullable()
                .WithColumn("supervisorname").AsString().Nullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("lastchangedate").AsDateTime().Nullable()
                .WithColumn("deviceid").AsString().Nullable()
                .WithColumn("personname").AsString().Nullable()
                .WithColumn("phonenumber").AsString().Nullable();

            Create.Table("roles")
                .WithColumn("userid").AsString(255).NotNullable()
                .WithColumn("roleid").AsInt32().Nullable();
        }
    }
}