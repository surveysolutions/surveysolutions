using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201902011754)]
    public class M201902011754_MigrateDataForNaNValuesForDoubleInTabulateReport : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"SELECT ids.*, r.*
                            FROM (SELECT distinct v.interviewid
                                    FROM readside.interviews v
                                   WHERE v.asdouble = 'NaN'::float
                                 ) as ids,
                         LATERAL readside.update_report_table_data(interviewid) r");
        }

        public override void Down()
        {

        }
    }
}
