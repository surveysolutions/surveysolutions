using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Events
{
    [Migration(0)]
    public class M000_Init : Migration
    {
        private const string schemaName = "events";
        private const string tableName = "events";

        public override void Up()
        {
                if (!Schema.Table(tableName).Exists())
                {

                    Execute.Sql(
                        $"CREATE TABLE IF NOT EXISTS {schemaName}.{tableName}(id uuid NOT NULL, origin text, \"timestamp\" timestamp without time zone NOT NULL, eventsourceid uuid NOT NULL, globalsequence integer NOT NULL, value json NOT NULL, eventsequence integer NOT NULL, eventtype text NOT NULL, CONSTRAINT pk PRIMARY KEY(id) ) WITH(OIDS = FALSE);");

                    Create.Index("event_source_indx")
                            .OnTable(tableName)
                            .OnColumn("eventsourceid");

                    Create.Index("globalsequence_indx")
                            .OnTable(tableName)
                            .OnColumn("globalsequence").Ascending().WithOptions().Unique();
                }
        }

        public override void Down()
        {
        }
        
    }
}
