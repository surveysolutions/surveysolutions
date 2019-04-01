using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903291300)]
    public class M201903291300_AddMissingIndexForCumulativeChartReport : Migration
    {
        public override void Up()
        {
            Create.Index("cumulativereportstatuschanges_interviewid_idx").OnTable("cumulativereportstatuschanges")
                .OnColumn("interviewid").Ascending();
        }

        public override void Down()
        {

        }
    }

    [Migration(201903291700)]
    public class M201903291700_InterviewSummaryNewPK : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"DROP INDEX readside.""PK_interviewsummaries"";
                ALTER TABLE readside.interviewsummaries DROP CONSTRAINT ""PK_interviewsummaries"";
                ALTER TABLE readside.interviewsummaries ADD CONSTRAINT interviewsummaries_pk PRIMARY KEY (id);
                CREATE INDEX interviewsummaries_summaryid_idx ON readside.interviewsummaries (summaryid);");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }

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

    public class M201904011135_Speedreportinterviewitems_Interview_Id : Migration {
        public override void Up()
        {
            //Execute.Sql(@"ALTER TABLE readside.speedreportinterviewitems ADD interview_id int4 NULL;

            //    update readside.speedreportinterviewitems c 
	           //     set interview_id = s.id
            //    from readside.interviewsummaries s
            //    where s.summaryid = c.interviewid;

            //    ALTER TABLE readside.speedreportinterviewitems ALTER COLUMN interview_id SET NOT NULL;

            //    CREATE INDEX speedreportinterviewitems_interviewid_idx ON readside.speedreportinterviewitems (interview_id);");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}
