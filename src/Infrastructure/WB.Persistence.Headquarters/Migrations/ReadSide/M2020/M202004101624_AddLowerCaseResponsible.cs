using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_04_10_16_24)]
    public class M202004101624_AddLowerCaseResponsible : Migration
    {
        public override void Up()
        {
            Create.Column("responsible_name_lower_case").OnTable("interviewsummaries")
                .AsString().Nullable();
            Create.Index("interviewsummaries_responsible_name_lower_case_idx").OnTable("interviewsummaries")
                .OnColumn("responsible_name_lower_case");
            
            Execute.Sql("UPDATE readside.interviewsummaries SET responsible_name_lower_case = LOWER(responsiblename)");
        }

        public override void Down()
        {
            Delete.Column("responsible_name_lower_case").FromTable("interviewsummaries");
        }
    }
}
