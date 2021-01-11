using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2020_12_03_13_44)]
    public class M202012031344_CreateMissingKeyValueTables : ForwardOnlyMigration
    {
        public override void Up()
        {
            string[] tables =
            {
                "questionnairedocuments",
                "questionnairebackups",
                "webinterviewconfigs",
                "questionnairepdfs",
                "profilesettings",
                "appsettings",
                "questionnairelookuptables",
                "emailparams"
            };

            foreach (var table in tables)
            {
                if (!Schema.Table(table).Exists())
                {
                    Create.CreateKeyValueTable(table);
                }
            }
        }
    }
}
