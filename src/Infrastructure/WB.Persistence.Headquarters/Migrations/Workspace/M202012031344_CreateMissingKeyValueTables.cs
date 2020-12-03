using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2020_12_03_13_44)]
    public class M202012031344_CreateMissingKeyValueTables : ForwardOnlyMigration
    {
        public override void Up()
        {
            if (!Schema.Table("questionnairedocuments").Exists())
            {
                Create.CreateKeyValueTable("questionnairedocuments");
            }

            if (!Schema.Table("questionnairebackups").Exists())
            {
                Create.CreateKeyValueTable("questionnairebackups");
            }
        }
    }
}
