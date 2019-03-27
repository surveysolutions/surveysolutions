using System.Diagnostics.CodeAnalysis;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903211302)]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class M201903211302_AddedNewStatisticsReportTable : Migration
    {
        public override void Up()
        {
            Create.Table("report_statistics")
                .WithColumn("id").AsCustom("serial").PrimaryKey()
                .WithColumn("interview_id").AsInt32().NotNullable()
                .WithColumn("entity_id").AsInt32().NotNullable()
                .WithColumn("rostervector").AsString()
                .WithColumn("answer").AsCustom("int8[]").NotNullable()
                .WithColumn("type").AsInt16().NotNullable().WithDefaultValue(0)
                .WithColumn("is_enabled").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.Index("idx_report_statistics_entityid").OnTable("report_statistics")
                .OnColumn("entity_id").Ascending();

            Execute.Sql(@"INSERT INTO readside.report_statistics (interview_id, entity_id, rostervector, answer, ""type"", is_enabled)
      SELECT isum.id as interview_id, qe.id AS entity_id, i.rostervector,
            case 
                WHEN i.asintarray IS NULL THEN ARRAY[COALESCE(i.asint::bigint, i.asdouble::bigint)]
                ELSE i.asintarray::bigint[]
            END AS answer,
            case when qe.question_type = 4 then 1 else 0 end as ""type"",
            i.isenabled as is_enabled
      FROM readside.interviews i 
      join readside.interviews_id id on id.id = i.interviewid
      JOIN readside.questionnaire_entities qe ON i.entityid = qe.id
      join readside.interviewsummaries isum on id.interviewid = isum.interviewid
      WHERE (i.asint IS NOT NULL 
            OR (i.asdouble IS NOT NULL and i.asdouble != 'NAN'::float8) 
            OR i.asintarray IS NOT NULL) 
        AND (qe.question_type = ANY (ARRAY[0, 3, 4])) 
        AND qe.linked_to_question_id IS NULL 
        AND qe.linked_to_roster_id IS NULL 
        AND qe.cascade_from_question_id IS NULL         
        AND qe.is_filtered_combobox = false");

            Execute.Sql(@"CREATE OR REPLACE VIEW readside.report_statistics_categorical
AS SELECT a.interview_id AS interview_id,
     a.entity_id,
     a.rostervector,
     ans.ans AS answer
   FROM readside.report_statistics a,
   LATERAL unnest(a.answer) ans(ans)
   where a.""type"" = 0 and is_enabled = true;");

            Execute.Sql(@"CREATE OR REPLACE VIEW readside.report_statistics_numeric
AS SELECT a.interview_id AS interview_id,
     a.entity_id,
     a.rostervector,
     ans.ans AS answer
   FROM readside.report_statistics a,
   LATERAL unnest(a.answer) ans(ans)
   where a.""type"" = 1 and is_enabled = true;");

          
        }

        public override void Down()
        {
            Delete.Table("report_statistics");
        }
    }
}
