using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(14)]
    public class M014_AddAnswerAsStringColumnForAssignments : Migration
    {
        public override void Up()
        {
            Create.Column("answerasstring").OnTable("assignmentsidentifyinganswers").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("answerasstring").FromTable("assignmentsidentifyinganswers");
        }
    }
}