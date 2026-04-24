using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_04_24_17_00)]
    public class M202604241700_AddUsedOneTimeCodesTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("used_one_time_codes")
                .WithColumn("code").AsString(64).PrimaryKey()
                .WithColumn("used_at").AsDateTimeOffset().NotNullable();
        }
    }
}
