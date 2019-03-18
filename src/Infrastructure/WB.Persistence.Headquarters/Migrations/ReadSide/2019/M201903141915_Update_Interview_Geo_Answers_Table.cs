using System.ComponentModel;
using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903141915)]
    [Localizable(false)]
    public class M201903141915_Update_Interview_Geo_Answers_Table : Migration
    {
        public override void Up()
        {
            Alter.Table("interview_geo_answers").AlterColumn("interviewid").AsFixedLengthString(255);
            Execute.Sql("UPDATE readside.interview_geo_answers SET interviewid= translate(interviewid, '-', '')");
            Create.ForeignKey("fk_interviewsummary_interview_geo_answer").FromTable("interview_geo_answers")
                .ForeignColumn("interviewid").ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
        }
    }
}
