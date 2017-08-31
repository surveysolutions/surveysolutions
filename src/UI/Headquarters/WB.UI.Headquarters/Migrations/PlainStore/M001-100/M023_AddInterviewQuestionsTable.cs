using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(23)]
    public class M023_AddInterviewQuestionsTable : Migration
    {
        public override void Up()
        {
            Create.Table("interviewquestions")
                .WithColumn("interviewid").AsGuid().PrimaryKey()
                .WithColumn("questionid").AsGuid().PrimaryKey()
                .WithColumn("rostervector").AsCustom("numeric[]").PrimaryKey()
                .WithColumn("isflagged").AsBoolean().WithDefaultValue(false);
            

            Create.Index("interviewquestions_interviewId")
                .OnTable("interviewquestions")
                .OnColumn("interviewid");

            // uncomment this in the future when interviewid will be primary key
            //Create.ForeignKey("fk_interviewquestions_interviewsummaries")
            //    .FromTable("interviewquestions")
            //    .ForeignColumn("interviewid")
            //    .ToTable("interviewsummaries").PrimaryColumn("interviewid");
        }

        public override void Down()
        {
            Delete.Table("interviewquestions");
            Delete.Index("interviewquestions_interviewId");
        }
    }
}