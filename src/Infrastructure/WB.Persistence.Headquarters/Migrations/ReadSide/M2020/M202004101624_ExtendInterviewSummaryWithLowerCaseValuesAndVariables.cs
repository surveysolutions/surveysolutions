using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_04_10_16_24)]
    public class M202004101624_ExtendInterviewSummaryWithLowerCaseValuesAndVariables : Migration
    {
        public override void Up()
        {
            Create.Column("responsible_name_lower_case").OnTable("interviewsummaries").AsString().Nullable();
            Create.Column("teamlead_name_lower_case").OnTable("interviewsummaries").AsString().Nullable();
            Create.Column("questionnaire_variable").OnTable("interviewsummaries").AsString().Nullable();
            
            if (Schema.Schema("plainstore").Table("questionnairebrowseitems").Exists())
            {
                Execute.Sql(@"update readside.interviewsummaries s
                    set 
                        responsible_name_lower_case = LOWER(responsiblename),
                        teamlead_name_lower_case = LOWER(teamleadname),
                        questionnaire_variable = coalesce(variable, q.questionnaireid::text) 
                    from (select variable, questionnaireid, ""version"" from plainstore.questionnairebrowseitems) q
                        where s.questionnaireid = q.questionnaireid  and questionnaireversion = q.""version""");
            }

            Create.Index("interviewsummaries_responsible_name_lower_case_idx")
                .OnTable("interviewsummaries").OnColumn("responsible_name_lower_case");
            
            Create.Index("interviewsummaries_teamlead_name_lower_case_idx")
                .OnTable("interviewsummaries").OnColumn("teamlead_name_lower_case");
            
            Execute.Sql("ALTER TABLE readside.interviewsummaries ALTER COLUMN questionnaire_variable SET NOT NULL;");
            Create.Index().OnTable("interviewsummaries").OnColumn("questionnaire_variable");
        }

        public override void Down()
        {
            Delete.Column("responsible_name_lower_case").FromTable("interviewsummaries");
            Delete.Column("teamlead_name_lower_case").FromTable("interviewsummaries");
        }
    }
}
