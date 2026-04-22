using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2025_12_12_14_47)]
    public class M202512121447_AddAppSettingsTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("appsettings")
                .WithColumn("id").AsCustom("text").PrimaryKey()
                .WithColumn("value").AsCustom("jsonb").NotNullable();
        }
    }
}
