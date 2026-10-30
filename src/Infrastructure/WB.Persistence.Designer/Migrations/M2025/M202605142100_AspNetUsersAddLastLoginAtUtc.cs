using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_05_14_21_00)]
    public class M202605142100_AspNetUsersAddLastLoginAtUtc : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("LastLoginAtUtc").OnTable("AspNetUsers").AsDateTime().Nullable();
        }
    }
}
