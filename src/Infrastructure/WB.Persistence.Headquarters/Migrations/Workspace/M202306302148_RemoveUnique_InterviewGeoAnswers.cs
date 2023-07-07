using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202306302148)]
    public class M202306302148_RemoveUnique_InterviewGeoAnswers : ForwardOnlyMigration
    {
        public override void Up()
        {
            var interviewGeoAnswers = "interview_geo_answers";
            var uniqueConstraintName = "UC_interview_geo_answers_interviewid_questionid_rostervector";

            if (Schema.Table(interviewGeoAnswers).Constraint(uniqueConstraintName).Exists())
            {
                Delete.UniqueConstraint(uniqueConstraintName)
                    .FromTable(interviewGeoAnswers);
            }
        }
    }
}
