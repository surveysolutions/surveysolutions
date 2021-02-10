using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202102041645)]
    public class M202102041645_AddReportingTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("interview_report_answers")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("entity_id").AsInt32()
                .WithColumn("interview_id").AsInt32().NotNullable()
                .WithColumn("value").AsString().Nullable()
                .WithColumn("value_lower_case").AsString().Nullable()
                .WithColumn("answer_code").AsInt32().Nullable()
                .WithColumn("enabled").AsBoolean().WithDefaultValue(true);

            Create.ForeignKey("fk_interview_report_answers_to_interviewsummaries")
                .FromTable("interview_report_answers").ForeignColumn("interview_id")
                .ToTable("interviewsummaries").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("FK_interview_report_answers_to_questionnaire_entities")
                .FromTable("interview_report_answers").ForeignColumn("entity_id")
                .ToTable("questionnaire_entities").PrimaryColumn("id")
                .OnDelete(Rule.None);
        }
    }
}
