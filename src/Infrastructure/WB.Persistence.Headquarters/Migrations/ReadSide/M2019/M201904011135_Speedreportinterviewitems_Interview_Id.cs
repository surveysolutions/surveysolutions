using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    public class M201904011135_Speedreportinterviewitems_Interview_Id : Migration {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE readside.speedreportinterviewitems ADD interview_id int4 NULL;

                update readside.speedreportinterviewitems c 
	                set interview_id = s.id
                from readside.interviewsummaries s
                where s.summaryid = c.interviewid;

                ALTER TABLE readside.speedreportinterviewitems ALTER COLUMN interview_id SET NOT NULL;

                CREATE INDEX speedreportinterviewitems_interviewid_idx ON readside.speedreportinterviewitems (interview_id);");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}