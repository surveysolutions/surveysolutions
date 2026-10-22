using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_10_13_20_30)]
    public class M202610132030_UniqueAnonymousQuestionnairePerQuestionnaire : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Execute.Sql(@"LOCK TABLE plainstore.anonymous_questionnaires IN ACCESS EXCLUSIVE MODE;
WITH ranked AS (
    SELECT anonymous_questionnaire_id,
           ROW_NUMBER() OVER (
               PARTITION BY questionnaire_id
               ORDER BY is_active DESC, generated_at_utc DESC, anonymous_questionnaire_id
           ) AS row_number
    FROM plainstore.anonymous_questionnaires
)
DELETE FROM plainstore.anonymous_questionnaires AS anonymous_questionnaire
USING ranked
WHERE anonymous_questionnaire.anonymous_questionnaire_id = ranked.anonymous_questionnaire_id
  AND ranked.row_number > 1;");

            this.Delete.Index("anonymous_questionnaires_questionnaire_id")
                .OnTable("anonymous_questionnaires");
            this.Create.UniqueConstraint("anonymous_questionnaires_questionnaire_id_unique")
                .OnTable("anonymous_questionnaires")
                .Column("questionnaire_id");
        }
    }
}
