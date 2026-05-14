using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_05_14_21_00)]
    public class M202605142100_AspNetUsersAddLastLoginDate : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("LastLoginDate").OnTable("AspNetUsers").AsDateTime().Nullable();
        }
    }
}
