using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201909091502)]
    public class M201909091502_AddAssignmentIdSequence : Migration
    {
        public override void Up()
        {
            Execute.Sql("CREATE SEQUENCE IF NOT EXISTS plainstore.assignment_id_sequence; ");

            if (Schema.Table("assignments").Exists())
                Execute.Sql("SELECT setval('plainstore.assignment_id_sequence', COALESCE(max(id), 1)) FROM plainstore.assignments; ");
        }

        public override void Down()
        {
            Delete.Sequence("assignment_id_sequence");
        }
    }
}
