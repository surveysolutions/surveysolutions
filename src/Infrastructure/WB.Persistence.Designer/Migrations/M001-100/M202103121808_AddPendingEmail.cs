using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2021_03_12_18_08)]
    public class M202103121808_AddPendingEmail : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("PendingEmail").OnTable("AspNetUsers").AsString(256).Nullable();
        }
    }
}
