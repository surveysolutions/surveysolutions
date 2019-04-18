using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903291700)]
    public class M201903291700_InterviewSummaryNewPK : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"    
                ALTER TABLE readside.interviewsummaries DROP CONSTRAINT unq_interviewsummaries_id;
                ALTER TABLE readside.interviewsummaries DROP CONSTRAINT ""PK_interviewsummaries"" cascade;
                ALTER TABLE readside.interviewsummaries ADD CONSTRAINT interviewsummaries_pk PRIMARY KEY (id);
                ALTER TABLE readside.interviewsummaries ADD CONSTRAINT unq_interviewsummaries_summaryid UNIQUE (summaryid);");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}