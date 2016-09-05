using System;
using FluentMigrator;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(5)]
    public class M005_AspNetUsers : Migration
    {
        public override void Up()
        {
            Create.Table("aspnetusers")
                .WithColumn("id").AsFixedLengthString(32).PrimaryKey()
                .WithColumn("email").AsString().Nullable()
                .WithColumn("passwordhash").AsString().Nullable()
                .WithColumn("phonenumber").AsString().Nullable()
                .WithColumn("isarchived").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("islockedbysupervisor").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("islockedbyheadquaters").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("username").AsString().NotNullable().Unique()
                .WithColumn("fullname").AsString().NotNullable();

            Create.Table("aspnetuserlogins")
                .WithColumn("userid").AsFixedLengthString(32)
                .WithColumn("loginprovider").AsString().NotNullable()
                .WithColumn("providerkey").AsString().NotNullable();

            Create.Table("aspnetuserclaims")
                .WithColumn("id").AsInt32()
                .WithColumn("userid").AsFixedLengthString(32)
                .WithColumn("claimtype").AsString().NotNullable()
                .WithColumn("claimvalue").AsString().NotNullable();

            Create.Table("aspnetroles")
                .WithColumn("id").AsFixedLengthString(2).PrimaryKey()
                .WithColumn("name").AsFixedLengthString(255).NotNullable().Unique();

            Create.Table("aspnetuserroles")
                .WithColumn("roleid").AsFixedLengthString(32).NotNullable()
                .WithColumn("userid").AsFixedLengthString(32).NotNullable();;

            foreach (int roleId in Enum.GetValues(typeof(UserRoles)))
            {
                Insert.IntoTable("aspnetroles").Row(new { id = roleId, name = Enum.GetName(typeof(UserRoles), roleId) });
            }
        }

        public override void Down()
        {
            Delete.Table("AspNetUsers");
            Delete.Table("AspNetUserLogins");
            Delete.Table("AspNetUserClaims");
            Delete.Table("AspNetRoles");
            Delete.Table("AspNetUserRoles");
        }
    }
}