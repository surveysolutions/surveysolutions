using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202003301500)]
    public class M202003301500_ExtendAnswersToFeaturedQuestions : Migration
    {
        public override void Up()
        {
            Alter.Table("answerstofeaturedquestions").InSchema("readside").AddColumn("question_id").AsInt32().Nullable();
            
            Create.ForeignKey()
                .FromTable("answerstofeaturedquestions").InSchema("readside").ForeignColumn("question_id")
                .ToTable("questionnaire_entities").InSchema("readside").PrimaryColumn("id");

            Create.ForeignKey()
               .FromTable("answerstofeaturedquestions").InSchema("readside").ForeignColumn("interview_id")
               .ToTable("interviewsummaries").InSchema("readside").PrimaryColumn("id");

            Alter.Table("answerstofeaturedquestions").InSchema("readside").AddColumn("answer_code").AsDecimal().Nullable();

            Execute.Sql(@"update readside.answerstofeaturedquestions ans
                set question_id = s.id, answer_code = s.answer_code 
                from (
	                select a.id as aid, qe.id as id, qea.answer_code as answer_code
	                from readside.answerstofeaturedquestions a
                    join readside.interviewsummaries s on s.id = a.interview_id 
	                join readside.questionnaire_entities qe on qe.questionnaireidentity = s.questionnaireidentity and qe.entityid = a.questionid 
	                left join readside._temp_questionnaire_entities_answers qea on qea.entity_id = qe.id and qea.text = a.answervalue 	
                ) s
                where ans.id = s.aid");
            
            Delete.Column("answertitle").FromTable("answerstofeaturedquestions").InSchema("readside");
            Delete.Column("questionid").FromTable("answerstofeaturedquestions").InSchema("readside");
        }

        public override void Down()
        {
        }
    }
}
