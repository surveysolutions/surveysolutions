using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2021_03_26_17_21)]
    public class M202103261721_FixSystemLogSequence : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Execute.Sql(@"
                SELECT setval('systemlog_id_seq', (SELECT MAX(id) FROM systemlog));
            ");
        }
    }
}
