using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201904011000)]
    public class M201904011000_MoveInterviewCommentedStatuses : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE readside.interviewcommentedstatuses ADD interview_id int4 NULL;

            CREATE INDEX interviewcommentedstatuses_interviewid_idx ON readside.interviewcommentedstatuses (interviewid);

            update readside.interviewcommentedstatuses c 
	            set interview_id = s.id
            from readside.interviewsummaries s
            where s.summaryid = c.interviewid;

            ALTER TABLE readside.interviewcommentedstatuses DROP COLUMN interviewid;

            ALTER TABLE readside.interviewcommentedstatuses ALTER COLUMN interview_id SET NOT NULL;

            CREATE INDEX interviewcommentedstatuses_interviewid_idx ON readside.interviewcommentedstatuses (interview_id);
            ");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}