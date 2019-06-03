using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Quartz
{
    [Migration(201905151013)]
    public class M201905151013_AddQuartzTables : Migration
    {
        public override void Up()
        {
            // source can be found https://raw.githubusercontent.com/quartznet/quartznet/master/database/tables/tables_postgres.sql

            Execute.EmbeddedScript("WB.Persistence.Headquarters.Migrations.Quartz.M201905151013_AddQuartzTables.sql");
        }

        public override void Down()
        {
        }
    }
}
