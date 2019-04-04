using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201903261800)]
    public class M201903261800_MigrateWebAssignments : Migration
    {
        public override void Up()
        {
            if (Schema.Table("webinterviewconfigs").Exists())
            {
                Execute.Sql(@"INSERT INTO plainstore.invitations (assignmentid, ""token"", numberofreminderssent)
                    select ass.id as assignmentid, ass.id::text as ""token"", 0 as numberofreminderssent
                    from plainstore.webinterviewconfigs config
                    join plainstore.assignments ass on ass.questionnaire = config.id
                    where (config.value ->> 'Started')::bool = true 
                        and not exists (select 1 from plainstore.invitations where assignmentid = ass.id)");
            }
        }

        public override void Down()
        {
            //throw new NotImplementedException();
        }
    }
}
