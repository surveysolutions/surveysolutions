using FluentMigrator;
namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(8)]
    public class M008_AddInterviewHumanId : Migration
    {

        public override void Up()
        {
            this.Create.Column(@"humanid").OnTable(@"interviewsummaries").AsInt32().Nullable();
            this.Create.Index(@"interviewsummaries_humanid").OnTable(@"interviewsummaries").OnColumn(@"humanid");
            
            this.Execute.Sql(Properties.Resources.add_human_interview_id);
        }

        public override void Down()
        {
            this.Delete.Column(@"humanid").FromTable(@"interviewsummaries");
            this.Delete.Index(@"interviewsummaries_humanid").OnTable(@"interviewsummaries");
            

            this.Execute.Sql(Properties.Resources.remove_human_interview_id);
        }
    }
}