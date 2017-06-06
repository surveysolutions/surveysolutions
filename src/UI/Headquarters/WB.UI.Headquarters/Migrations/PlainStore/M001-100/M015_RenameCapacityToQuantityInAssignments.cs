using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(15)]
    public class M015_RenameCapacityToQuantityInAssignments : Migration
    {
        public override void Up()
        {
            Rename.Column("capacity").OnTable("assignments").To("quantity");
        }

        public override void Down()
        {
            Rename.Column("quantity").OnTable("assignments").To("capacity");
        }
    }
}