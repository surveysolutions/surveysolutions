using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202003230925)]
    public class M202003230925_FixIndexesForIntervieCommentedStatus : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE readside.interviewcommentedstatuses 
                ADD CONSTRAINT PK_interviewcommentedstatuses PRIMARY KEY 
                USING INDEX ""UniqueIdIndex"";");

            Execute.Sql(@"
                DROP INDEX readside.interviewcommentedstatuses_interviewid_idx;
                DROP INDEX readside.interviewcommentedstatuses_status_indx;
                DROP INDEX readside.interviewcommentedstatuses_timestamp;");

            Execute.Sql(@"CREATE INDEX interviewcommentedstatuses_interview_id_idx 
                ON readside.interviewcommentedstatuses (interview_id,""position"");");
        }

        public override void Down()
        {
            
        }
    }
}
