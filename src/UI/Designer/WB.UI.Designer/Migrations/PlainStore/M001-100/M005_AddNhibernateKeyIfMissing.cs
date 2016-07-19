using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(5)]
    public class M005_AddNhibernateKeyIfMissing : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("hibernate_unique_key").Exists())
            {
                Create.Table("hibernate_unique_key")
                    .WithColumn("next_hi").AsInt32();

                Insert.IntoTable("hibernate_unique_key").Row(new {next_hi = 1});
            }
        }

        public override void Down()
        {
            if (Schema.Table("hibernate_unique_key").Exists())
            {
                Delete.Table("hibernate_unique_key");
            }
        }
    }
}