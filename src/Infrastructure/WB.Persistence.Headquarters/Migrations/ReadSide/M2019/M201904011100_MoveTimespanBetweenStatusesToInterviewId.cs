using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    public class M201904011100_MoveTimespanBetweenStatusesToInterviewId : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE readside.timespanbetweenstatuses ADD interview_id int4 NULL;

                CREATE INDEX timespanbetweenstatuses_interview_idx ON readside.timespanbetweenstatuses (interviewid);

                update readside.timespanbetweenstatuses c 
	                set interview_id = s.id
                from readside.interviewsummaries s
                where s.summaryid = c.interviewid;

                ALTER TABLE readside.timespanbetweenstatuses DROP COLUMN interviewid;

                ALTER TABLE readside.timespanbetweenstatuses ALTER COLUMN interview_id SET NOT NULL;

                CREATE INDEX timespanbetweenstatuses_interviewid_idx ON readside.timespanbetweenstatuses (interview_id);");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}