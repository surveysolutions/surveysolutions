using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(15)]
    public class M015_AddAllowedAddressesTable : Migration
    {
        public override void Up()
        {
            if (!this.Schema.Table("allowedaddresses").Exists())
            {
                this.Create.Table("allowedaddresses")
                    .WithColumn("id").AsInt32().PrimaryKey()
                    .WithColumn("description").AsString().Nullable()
                    .WithColumn("address").AsString().NotNullable();
            }
        }

        public override void Down()
        {
            this.Delete.Table("allowedaddresses");
        }
    }
}