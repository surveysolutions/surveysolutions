using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(202005041726, "Removed allowed addresses", BreakingChange = false)]
    public class M202005041726_RemoveAllowedAddresses : Migration
    {
        public override void Up()
        {
            Delete.Table("allowedaddresses");
        }

        public override void Down()
        {
        }
    }
}
