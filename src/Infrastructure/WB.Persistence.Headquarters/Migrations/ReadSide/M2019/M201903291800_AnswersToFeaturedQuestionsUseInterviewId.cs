using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903291800)]
    public class M201903291800_AnswersToFeaturedQuestionsUseInterviewId : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE readside.answerstofeaturedquestions ADD interview_id int4 NULL;

                update readside.answerstofeaturedquestions 
	                set interview_id = s.id
                from readside.interviewsummaries s
                where s.summaryid = interviewsummaryid;

                ALTER TABLE readside.answerstofeaturedquestions DROP COLUMN interviewsummaryid;

                ALTER TABLE readside.answerstofeaturedquestions ALTER COLUMN interview_id SET NOT NULL;");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}