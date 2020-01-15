using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(201912191055)]
    public class M201912191055_DeleteRoleClaims : Migration
    {
        public override void Up()
        {
            Delete.FromTable("userclaims").Row(new {ClaimType = "observer"});
            Delete.FromTable("userclaims").Row(new {ClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"});
        }

        public override void Down()
        {
        }
    }
}
