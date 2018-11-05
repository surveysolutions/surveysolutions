using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201808131420)]
    public class M201808131420_AddIndicesForInterviewSummariesAndComments : Migration
    {
        public override void Up()
        {
            Create.Index("interviewsummaries_interviewid_idx")
                .OnTable("interviewsummaries")
                .OnColumn("interviewid")
                .Ascending();

            Execute.Sql(@"CREATE INDEX commentaries_variable_idx ON readside.commentaries (variable text_pattern_ops);");
        }

        public override void Down()
        {
            Delete.Index("interviewsummaries_interviewid_idx").OnTable("interviewsummaries");
            Delete.Index("commentaries_variable_idx").OnTable("commentaries");
        }
    }
}
