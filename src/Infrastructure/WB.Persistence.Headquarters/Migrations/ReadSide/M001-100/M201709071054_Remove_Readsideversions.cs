using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201709071054)]
    public class M201709071054_Remove_Readsideversions : Migration
    {
        public override void Up()
        {
            if (Schema.Table("readsideversions").Exists())
                Delete.Table("readsideversions");
        }

        public override void Down()
        {
        }
    }
}