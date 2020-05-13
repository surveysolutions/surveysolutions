using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202004241423)]
    public class M202004241423_AddHasSmallSubstitutionsColumnToInterviewSummary : Migration
    {
        public override void Up()
        {
            this.Execute.Sql(@"ALTER TABLE readside.interviewsummaries ADD hassmallsubstitutions bool DEFAULT false;");
            this.Execute.Sql(@"ALTER TABLE readside.interviewsummaries ALTER COLUMN hassmallsubstitutions SET NOT NULL");
        }

        public override void Down()
        {
            Delete.Column("hassmallsubstitutions").FromTable("interviewsummaries").InSchema("readside");
        }
    }
}
