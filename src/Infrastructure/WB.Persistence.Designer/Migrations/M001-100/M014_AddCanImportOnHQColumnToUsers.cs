using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(14)]
    public class M014_AddCanImportOnHQColumnToUsers : Migration
    {
        public override void Up()
        {
            this.Create.Column("canimportonhq")
                .OnTable("users")
                .AsBoolean()
                .Nullable();
        }

        public override void Down()
        {
            this.Delete.Column("canimportonhq").FromTable("users");
        }
    }
}