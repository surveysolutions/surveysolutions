using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Events
{
    [Migration(1)]
    public class M001_AddEventSequenceIndex : Migration
    {
        private const string schemaName = "events";
        private const string tableName = "events";

        public override void Up()
        {
            if (Schema.Table(tableName).Exists())
            {
                Execute.Sql(
                    $" delete from {schemaName}.{tableName} where exists(select 1 from {schemaName}.{tableName} ee2 where {schemaName}.{tableName}.eventsourceid = ee2.eventsourceid AND {schemaName}.{tableName}.eventsequence = ee2.eventsequence AND {schemaName}.{tableName}.globalsequence < ee2.globalsequence);");

                if (!Schema.Table(tableName).Index("event_source_eventsequence_indx").Exists())
                {
                    Create.Index("event_source_eventsequence_indx")
                        .OnTable(tableName)
                        .OnColumn("eventsourceid").Ascending()
                        .OnColumn("eventsequence").Ascending().WithOptions().Unique();
                }
            }
        }

        public override void Down()
        {
        }
        
    }
}
