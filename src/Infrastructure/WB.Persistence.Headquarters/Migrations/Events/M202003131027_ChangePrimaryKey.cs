using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Events
{
    [Migration(2020_03_13_10_27)]
    public class M202003131027_ChangePrimaryKey : Migration
    {
        public override void Up()
        {
            Delete.PrimaryKey("pk").FromTable("events").InSchema("events");

            // This is fastest way. https://dba.stackexchange.com/questions/8814/how-to-promote-an-existing-index-to-primary-key-in-postgresql
            Execute.Sql("ALTER TABLE events.events ADD CONSTRAINT events_pk PRIMARY KEY USING INDEX globalsequence_indx;");
        }

        public override void Down()
        {
        }
    }
}
