using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202003230900)]
    public class M202003230900_AddMissingIndexesForAnswersOnFeaturedQuesions : Migration
    {
        public override void Up()
        {
            // No longer required. Later migration will rewrite answerstofeaturedquestions table 
            
            //             Execute.Sql(@"
            //                 DROP INDEX IF EXISTS readside.answerstofeaturedquestions_answervalue_idx;
            //
            //                 CREATE INDEX IF NOT EXISTS answerstofeaturedquestions_interview_id_position_idx 
            //                     ON readside.answerstofeaturedquestions USING btree(interview_id, ""position"")");
        }

        public override void Down()
        {
        }
    }
}
