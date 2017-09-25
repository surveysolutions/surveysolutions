using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201709251028)]
    public class M201709251028_AddWasCompletedFlagToSummary : Migration
    {
        public override void Up()
        {
            Create.Column("wascompleted").OnTable("interviewsummaries").AsBoolean().NotNullable()
                .WithDefaultValue(false);
            Execute.Sql(@"
                    UPDATE readside.interviewsummaries s
                    SET (wascompleted) =
                        (SELECT COALESCE((value->> 'WasCompleted')::boolean, false)
                        FROM readside.interviewdatas d
                        WHERE s.summaryid = d.id)");
        }

        public override void Down()
        {
            Delete.Column("wascompleted").FromTable("interviewsummaries");
        }
    }
}