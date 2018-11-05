using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(4)]
    public class M004_RemoveOldMigrationSchemas : Migration
    {
        public override void Up()
        {
            if (Schema.Table("migrations").Exists())
                this.Delete.Table("migrations");

            if (Schema.Table("dataMigrations").Exists())
                this.Delete.Table("dataMigrations");
        }

        public override void Down()
        {
            // Both tables will be rebuilded automatically if application downgraded
        }
    }
}