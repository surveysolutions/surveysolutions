using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201904011925)]
    public class M201904011925_SetDefaultValueForReminders : Migration
    {
        public override void Up()
        {
            if (Schema.Table("webinterviewconfigs").Exists())
            {
                Execute.Sql(@"UPDATE plainstore.webinterviewconfigs SET value = value::jsonb || jsonb '{""ReminderAfterDaysIfPartialResponse"":3, ""ReminderAfterDaysIfNoResponse"":3}'");
            }
        }

        public override void Down()
        {
        }
    }
}
