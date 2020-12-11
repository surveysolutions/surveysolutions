using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(201911261359)]
    public class M201911261359_AddFreeStorageToProfile : Migration
    {
        public override void Up()
        {
            Create.Column("StorageFreeInBytes").OnTable("userprofiles").AsDouble().Nullable();
            if (Schema.Schema("plainstore").Table("devicesyncinfo").Exists())
            {
                Execute.Sql(@"
                update users.userprofiles up
                set ""StorageFreeInBytes"" = 
                    (select dsi.""StorageFreeInBytes"" 
                from 
                    users.users u 
                left join users.userprofiles up1 on u.""UserProfileId"" = up1.""Id""
                left join plainstore.devicesyncinfo dsi on dsi.""InterviewerId"" = u.""Id"" 
                where up.""Id"" = up1.""Id""
                order by dsi.""Id"" desc
                    limit 1)");
            }
        }

        public override void Down()
        {
            Delete.Column("StorageFreeInBytes").FromTable("userprofiles");
        }
    }
}
