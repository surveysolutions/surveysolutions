using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201805101216)]
    public class M201805101216_AddProtectedVariables : Migration
    {
        public override void Up()
        {
            Alter.Table("assignmenttoimport").AddColumn("protectedvariables").AsCustom("jsonb").Nullable();
            Alter.Table("assignments").AddColumn("protectedvariables").AsCustom("jsonb").Nullable();
        }

        public override void Down()
        {
            Delete.Column("protectedvariables").FromTable("assignmenttoimport");
            Delete.Column("protectedvariables").FromTable("assignments");
        }
    }
}
