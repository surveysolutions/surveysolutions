using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_04_24_17_00)]
    public class M202604241700_AddUsedOneTimeCodesTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("used_one_time_codes")
                .WithColumn("code").AsString(128).PrimaryKey()
                .WithColumn("used_at").AsDateTimeOffset().NotNullable()
                .WithColumn("expires_at").AsDateTimeOffset().NotNullable();

            Create.Index("ix_used_one_time_codes_expires_at")
                .OnTable("used_one_time_codes")
                .OnColumn("expires_at");
        }
    }
}
