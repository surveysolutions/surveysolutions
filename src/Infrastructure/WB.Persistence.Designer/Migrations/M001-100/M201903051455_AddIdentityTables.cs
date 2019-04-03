using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201903051455)]
    // https://gist.github.com/kidchenko/2afd260a510bacec70a0321e934285bd
    public class M201903051455_AddIdentityTables : Migration
    {
        public override void Up()
        {
            Create.Table("AspNetRoles")
               .WithColumn("Id").AsString().PrimaryKey("PK_AspNetRoles").NotNullable()
               .WithColumn("ConcurrencyStamp").AsString().Nullable()
               .WithColumn("Name").AsString(256).NotNullable()
               .WithColumn("NormalizedName").AsString(256).Nullable()
                                            .Indexed("RoleNameIndex");


            Create.Table("AspNetUserTokens")
                .WithColumn("UserId").AsString().PrimaryKey("PK_AspNetUserTokens").NotNullable()
                .WithColumn("LoginProvider").AsString()
                .WithColumn("Name").AsString()
                .WithColumn("Value").AsString();

            Create.Table("AspNetUsers")
               .WithColumn("Id").AsString().NotNullable().PrimaryKey("PK_AspNetUsers")
               .WithColumn("AccessFailedCount").AsInt32().NotNullable()
               .WithColumn("ConcurrencyStamp").AsString().Nullable()
               .WithColumn("Email").AsString(256).Nullable()
               .WithColumn("EmailConfirmed").AsBoolean().NotNullable()
               .WithColumn("LockoutEnabled").AsBoolean().NotNullable()
               .WithColumn("LockoutEnd").AsDateTimeOffset().Nullable()
               .WithColumn("NormalizedEmail").AsString().Nullable()
               .WithColumn("NormalizedUserName").AsString().Nullable()
               .WithColumn("PasswordHash").AsString().Nullable()
               .WithColumn("PhoneNumber").AsString().Nullable()
               .WithColumn("PhoneNumberConfirmed").AsBoolean().NotNullable()
               .WithColumn("SecurityStamp").AsString().Nullable()
               .WithColumn("TwoFactorEnabled").AsBoolean().NotNullable()
               .WithColumn("UserName").AsString(256).Nullable();


            Create.Table("AspNetRoleClaims")
                .WithColumn("Id").AsInt32().PrimaryKey("PK_AspNetRoleClaims").Identity()
                .WithColumn("ClaimType").AsString().Nullable()
                .WithColumn("ClaimValue").AsString().Nullable()
                .WithColumn("RoleId").AsString().NotNullable().Indexed("IX_AspNetRoleClaims_RoleId")
                                     .ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoles", "Id");

            Create.Table("AspNetUserClaims")
              .WithColumn("Id").AsInt32().PrimaryKey("PK_AspNetUserClaims").Identity()
              .WithColumn("ClaimType").AsString().Nullable()
              .WithColumn("ClaimValue").AsString().Nullable()
              .WithColumn("UserId").AsString().NotNullable().Indexed("IX_AspNetUserClaims_UserId")
                                   .ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUsers", "Id")
                                   .OnDelete(System.Data.Rule.Cascade);

            Create.Table("AspNetUserLogins")
              .WithColumn("LoginProvider").AsString().NotNullable().PrimaryKey("PK_AspNetUserLogins")
              .WithColumn("ProviderKey").AsString().NotNullable().PrimaryKey("PK_AspNetUserLogins")
              .WithColumn("ProviderDisplayName").AsString().Nullable()
              .WithColumn("UserId").AsString()
                                   .NotNullable()
                                   .Indexed("IX_AspNetUserLogins_UserId")
                                   .ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUsers", "Id")
                                   .OnDelete(System.Data.Rule.Cascade);


            Create.Table("AspNetUserRoles")
              .WithColumn("UserId").AsString()
                                   .PrimaryKey("PK_AspNetUserRoles")
                                   .Indexed("IX_AspNetUserRoles_UserId")
                                   .ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUsers", "Id")

              .WithColumn("RoleId").AsString()
                                   .PrimaryKey("PK_AspNetUserRoles")
                                   .Indexed("IX_AspNetUserRoles_RoleId")
                                   .ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetRoles", "Id")
                                   .OnDelete(System.Data.Rule.Cascade);
        }

        public override void Down()
        {
        }
    }
}
