using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(202106111218)]
    public class M202106111218_UpdateResetPasswordFlagForApiUsers : Migration
    {
        public override void Up()
        {
            this.Execute.Sql(@"
UPDATE users.users u
	SET password_change_required=false
from users.userroles r 
where r.""RoleId"" = '00000000-0000-0000-0000-000000000008' 
    and r.""UserId"" = u.""Id"" 
    and u.password_change_required=true ");
        }

        public override void Down()
        {
            
        }
    }
}
