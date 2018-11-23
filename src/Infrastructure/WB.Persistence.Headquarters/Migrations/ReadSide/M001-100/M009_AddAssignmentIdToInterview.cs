using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(9)]
    public class M009_AddAssignmentIdToInterview : Migration
    {
        public override void Up()
        {
            Create.Column(@"assignmentid").OnTable(@"interviewsummaries").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete.Column(@"assignmentid").FromTable(@"interviewsummaries");
        }
    }
}