using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202009091228)]
    public class M202009091228_CleareDataForNotExistedInterviews : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                DELETE FROM readside.interview_geo_answers as g
                    WHERE NOT EXISTS ( SELECT * FROM readside.interviewsummaries as i
                        WHERE g.interview_id = i.id 
                    );
                DELETE FROM readside.report_statistics as r
                    WHERE NOT EXISTS ( SELECT * FROM readside.interviewsummaries as i
                        WHERE r.interview_id = i.id 
                    );
                DELETE FROM readside.cumulativereportstatuschanges as c
                    WHERE NOT EXISTS ( SELECT * FROM readside.interviewsummaries as i
                        WHERE c.interviewid = i.interviewid 
                    );
                DELETE FROM readside.commentaries as c
                    WHERE NOT EXISTS ( SELECT * FROM readside.interviewsummaries as i
                        WHERE c.summary_id = i.id 
                    );
            ");
        }

        public override void Down()
        {
            
        }
    }
}
