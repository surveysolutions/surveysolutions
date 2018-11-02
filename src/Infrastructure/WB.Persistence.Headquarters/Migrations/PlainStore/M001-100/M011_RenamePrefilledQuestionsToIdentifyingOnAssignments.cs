using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(11)]
    public class M011_RenamePrefilledQuestionsToIdentifyingOnAssignments : Migration
    {
        private const string assignmentsIdentifyingAnswers = "assignmentsidentifyinganswers";
        private const string assignmentsPrefilledAnswers = "assignmentsprefilledanswers";

        public override void Up()
        {
            this.Delete.Index("assignmentsprefilledanswers_assignments").OnTable(assignmentsPrefilledAnswers);

            this.Rename.Table(assignmentsPrefilledAnswers).To(assignmentsIdentifyingAnswers);
            this.Create.Index("assignmentsidentifyinganswers_assignments")
                .OnTable(assignmentsIdentifyingAnswers)
                .OnColumn("assignmentid")
                .Ascending();
        }

        public override void Down()
        {
            this.Rename.Table(assignmentsIdentifyingAnswers).To(assignmentsPrefilledAnswers);

            this.Delete.Index("assignmentsidentifyinganswers_assignments").OnTable(assignmentsPrefilledAnswers);
            this.Create.Index("assignmentsprefilledanswers_assignments")
                .OnTable(assignmentsPrefilledAnswers)
                .OnColumn("assignmentid")
                .Ascending();
        }
    }
}