using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_04_16_16_24)]
    public class M202004161624_AddLowerCaseTeamLead : Migration
    {
        public override void Up()
        {
            Create.Column("teamlead_name_lower_case").OnTable("interviewsummaries")
                .AsString().Nullable();
            Create.Index("interviewsummaries_teamlead_name_lower_case_idx").OnTable("interviewsummaries")
                .OnColumn("teamlead_name_lower_case");
            
            Execute.Sql("UPDATE readside.interviewsummaries SET teamlead_name_lower_case = LOWER(teamleadname)");
        }

        public override void Down()
        {
            Delete.Column("teamlead_name_lower_case").FromTable("interviewsummaries");
        }
    }
}
