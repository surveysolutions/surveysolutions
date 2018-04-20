using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201804171241)]
    public class M201804171241_ChangeHasErrorsToNumberOfErrors : Migration
    {
        public override void Up()
        {
            Create.Column("errorscount").OnTable("interviewsummaries").AsInt32().SetExistingRowsTo(0).NotNullable();
            Execute.Sql(
              @"update readside.interviewsummaries s set errorscount = (
                select COUNT(iv.interviewid)
                from readside.interviews_view iv
                    where iv.invalidvalidations is not null 
                          and iv.interviewid = s.interviewid
                    )");

            Delete.Column("haserrors").FromTable("interviewsummaries");
        }

        public override void Down()
        {
            Create.Column("haserrors").OnTable("interviewsummaries").AsBoolean().SetExistingRowsTo(false).Nullable();
            Delete.Column("errorscount").FromTable("interviewsummaries");
        }
    }
}
