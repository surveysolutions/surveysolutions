using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201709071054)]
    public class M201709071054_Remove_Readsideversions : Migration
    {
        public override void Up()
        {
            Delete.Table("readsideversions");
        }

        public override void Down()
        {
        }
    }
}