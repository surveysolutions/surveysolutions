using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201805031628)]
    public class M201805031628_AllowNullForResponsibleName : Migration
    {
        public override void Up()
        {
            Alter.Table("auditlogrecords")
                .AlterColumn("responsiblename").AsString().Nullable();
        }

        public override void Down()
        {
            Alter.Table("auditlogrecords")
                .AlterColumn("responsiblename").AsString();
        }
    }
}
