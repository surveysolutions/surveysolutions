using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201806271400)]
    public class M201806271400_AddUniqueConstraintOnInterviewEntities : Migration
    {
        public override void Up()
        {
            Create.Index("interviews_unique_index").OnTable("interviews").OnColumn("interviewid").Ascending()
                .OnColumn("entityid").Ascending()
                .OnColumn("rostervector").Unique();

            Delete.Index("interviews_interview_id_idx").OnTable("interviews");
        }

        public override void Down()
        {
            Delete.Index("interviews_unique_index").OnTable("interviews");
            Create.Index("interviews_interview_id_idx").OnTable("interviews").OnColumn("interviewid");
        }
    }
}
