using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2021_03_22_19_21)]
    public class M202103221921_FixSequencesInLogs : Migration
    {
        public override void Up()
        {
            this.Execute.Sql(@"
                SELECT setval('auditlogrecords_id_seq', (SELECT MAX(id) FROM auditlogrecords));
                SELECT setval('systemlog_id_seq', (SELECT MAX(id) FROM systemlog));
            ");
        }

        public override void Down()
        {
            
        }
    }
}
