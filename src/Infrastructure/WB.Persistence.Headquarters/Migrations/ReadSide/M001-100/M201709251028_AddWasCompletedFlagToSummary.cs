using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201709251028)]
    public class M201709251028_AddWasCompletedFlagToSummary : Migration
    {
        public override void Up()
        {
            Create.Column("wascompleted").OnTable("interviewsummaries").AsBoolean().Nullable()
                .WithDefaultValue(false);
            if (Schema.Table("interviewdatas").Exists())
            {
                Execute.Sql(@"
                    UPDATE readside.interviewsummaries s
                    SET (wascompleted) =
                        (SELECT COALESCE((value->> 'WasCompleted')::boolean, false)
                        FROM readside.interviewdatas d
                        WHERE s.summaryid = d.id)");
            }
            Update.Table("interviewsummaries").Set(new { wascompleted = false}).Where(new { wascompleted = (bool?)null});
            Alter.Column("wascompleted").OnTable("interviewsummaries").AsBoolean().NotNullable();
        }

        public override void Down()
        {
            Delete.Column("wascompleted").FromTable("interviewsummaries");
        }
    }
}