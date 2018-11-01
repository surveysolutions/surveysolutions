using System.ComponentModel;
using System.Data;
using FluentMigrator;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(1)]
    [Localizable(false)]
    public class M001_AddUsersHqIdentityModel : Migration
    {
        public override void Up()
        {
            this.CreateTableIfNotExists("roles", c => c
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString(256).NotNullable().Unique("RoleNameIndex"));

            this.CreateTableIfNotExists("userroles", t => t
                .WithColumn("UserId").AsGuid().PrimaryKey().Indexed()
                .WithColumn("RoleId").AsGuid().PrimaryKey().Indexed());

            this.CreateTableIfNotExists("users", t => t
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("UserProfileId").AsInt32().Nullable().Indexed()
                .WithColumn("FullName").AsString().Nullable()
                .WithColumn("IsArchived").AsBoolean().NotNullable()
                .WithColumn("IsLockedBySupervisor").AsBoolean().NotNullable()
                .WithColumn("IsLockedByHeadquaters").AsBoolean().NotNullable()
                .WithColumn("CreationDate").AsDateTime().NotNullable()
                .WithColumn("PasswordHashSha1").AsString().Nullable()
                .WithColumn("Email").AsString(256).Nullable()
                .WithColumn("EmailConfirmed").AsBoolean().NotNullable()
                .WithColumn("PasswordHash").AsString().Nullable()
                .WithColumn("SecurityStamp").AsString().Nullable()
                .WithColumn("PhoneNumber").AsString().Nullable()
                .WithColumn("PhoneNumberConfirmed").AsBoolean().NotNullable()
                .WithColumn("TwoFactorEnabled").AsBoolean().NotNullable()
                .WithColumn("LockoutEndDateUtc").AsDateTime().Nullable()
                .WithColumn("LockoutEnabled").AsBoolean().NotNullable()
                .WithColumn("AccessFailedCount").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(256).NotNullable().Unique("UserNameIndex"));

            this.CreateTableIfNotExists("userclaims", t => t
                .WithColumn("Id").AsInt32().NotNullable().Identity().PrimaryKey()
                .WithColumn("UserId").AsGuid().NotNullable().Indexed()
                .WithColumn("ClaimType").AsString()
                .WithColumn("ClaimValue").AsString());

            this.CreateTableIfNotExists("userlogins", t => t
                .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey()
                .WithColumn("ProviderKey").AsString(128).NotNullable().PrimaryKey()
                .WithColumn("UserId").AsGuid().NotNullable().PrimaryKey().Indexed());

            this.CreateTableIfNotExists("userprofiles", t => t
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("DeviceId").AsString().Nullable()
                .WithColumn("SupervisorId").AsGuid().Nullable());

            this.CreateForeignKeyFromTableIfNotExists("FK_users.userroles_users.users_UserId", "userroles", f => f.ForeignColumn("UserId")
                .ToTable("users").PrimaryColumn("Id")
                .OnDelete(Rule.Cascade));

            this.CreateForeignKeyFromTableIfNotExists("FK_users.userroles_users.roles_RoleId", "userroles", f => f.ForeignColumn("RoleId")
                .ToTable("roles").PrimaryColumn("Id")
                .OnDelete(Rule.Cascade));

            this.CreateForeignKeyFromTableIfNotExists("FK_users.users_users.userprofiles_UserProfileId", "users", f => f.ForeignColumn("UserProfileId")
                .ToTable("userprofiles").PrimaryColumn("Id")
                .OnDelete(Rule.None));

            this.CreateForeignKeyFromTableIfNotExists("FK_users.userclaims_users.users_UserId", "userclaims", f => f.ForeignColumn("UserId")
                 .ToTable("users").PrimaryColumn("Id")
                 .OnDelete(Rule.Cascade));

            this.CreateForeignKeyFromTableIfNotExists("FK_users.userlogins_users.users_UserId", "userlogins", f => f.ForeignColumn("UserId")
                .ToTable("users").PrimaryColumn("Id")
                .OnDelete(Rule.Cascade));
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_users.userlogins_users.users_UserId");
            Delete.ForeignKey("FK_users.userclaims_users.users_UserId");
            Delete.ForeignKey("FK_users.users_users.userprofiles_UserProfileId");
            Delete.ForeignKey("FK_users.userroles_users.roles_RoleId");
            Delete.ForeignKey("FK_users.userroles_users.users_UserId");

            Delete.Index().OnTable("userlogins").OnColumn("UserId");
            Delete.Index().OnTable("userclaims").OnColumn("UserId");
            Delete.Index("UserNameIndex");
            Delete.Index().OnTable("users").OnColumn("UserProfileId");
            Delete.Index().OnTable("userroles").OnColumn("RoleId");
            Delete.Index().OnTable("userroles").OnColumn("UserId");
            Delete.Index("RoleNameIndex");

            Delete.Table("userprofiles");
            Delete.Table("userlogins");
            Delete.Table("userclaims");
            Delete.Table("users");
            Delete.Table("userroles");
            Delete.Table("roles");
        }
    }
}