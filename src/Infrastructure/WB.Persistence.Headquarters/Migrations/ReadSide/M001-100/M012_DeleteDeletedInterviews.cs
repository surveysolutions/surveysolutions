using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(12)]
    public class M012_DeleteDeletedInterviews : Migration
    {
        public override void Up()
        {
            Delete.ForeignKey("fk_interviewsummaries_answerstofeaturedquestions").OnTable("answerstofeaturedquestions");

            Create.ForeignKey("fk_interviewsummaries_answerstofeaturedquestions")
                .FromTable("answerstofeaturedquestions").ForeignColumn("interviewsummaryid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);

            Delete.FromTable("interviewsummaries").Row(new { isdeleted = true});
            Delete.Column("isdeleted").FromTable("interviewsummaries");
        }

        public override void Down()
        {
            Create.Column("isdeleted").OnTable("interviewsummaries").AsBoolean().WithDefaultValue(false);
        }
    }
}