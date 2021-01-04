using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.MigrateToPrimaryWorkspace
{
    [Migration(2020_11_13_13_52)]
    public class M202011131352_DeleteOldSchemas : Migration
    {
        public override void Up()
        {
            string sql = "DROP SCHEMA IF EXISTS {0} CASCADE";
            string[] schemas = {"readside", "plainstore", "events"};

            foreach (var schema in schemas)
            {
                Execute.Sql(string.Format(sql, schema));
            }
        }

        public override void Down()
        {
        }
    }
}
