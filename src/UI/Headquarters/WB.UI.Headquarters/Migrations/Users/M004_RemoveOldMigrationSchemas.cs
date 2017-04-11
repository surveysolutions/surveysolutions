using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.Users
{
    [Migration(4)]
    public class M004_RemoveOldMigrationSchemas : Migration
    {
        public override void Up()
        {
            this.Delete.Table("migrations");
            this.Delete.Table("dataMigrations");
        }

        public override void Down()
        {
            // Both tables will be rebuilded automatically if application downgraded
        }
    }
}