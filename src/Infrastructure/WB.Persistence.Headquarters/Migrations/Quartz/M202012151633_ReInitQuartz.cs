using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Quartz
{
    [Migration(2020_12_15_16_33)]
    public class M202012151633_ReInitQuartz : ForwardOnlyMigration
    {
        public override void Up()
        {
            Delete.FromTable("fired_triggers").AllRows();
            Delete.FromTable("paused_trigger_grps").AllRows();
            Delete.FromTable("scheduler_state").AllRows();
            Delete.FromTable("locks").AllRows();
            Delete.FromTable("simprop_triggers").AllRows();
            Delete.FromTable("simple_triggers").AllRows();
            Delete.FromTable("cron_triggers").AllRows();
            Delete.FromTable("blob_triggers").AllRows();
            Delete.FromTable("triggers").AllRows();
            Delete.FromTable("job_details").AllRows();
            Delete.FromTable("calendars").AllRows();
        }
    }
}
