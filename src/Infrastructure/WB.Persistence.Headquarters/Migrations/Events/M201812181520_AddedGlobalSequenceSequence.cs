using System.Diagnostics.CodeAnalysis;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Events
{
    [Migration(201812181520)]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class M201812181520_AddedGlobalSequenceSequence : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"CREATE SEQUENCE if not exists events.globalsequence INCREMENT BY 1 MINVALUE 1 NO MAXVALUE CACHE 1 NO CYCLE");
            Execute.Sql(@"select setval('events.globalsequence',  (SELECT MAX(globalsequence) FROM events.events));");
            Execute.Sql(@"ALTER TABLE events.events ALTER COLUMN globalsequence SET DEFAULT nextval('events.globalsequence');");
        }

        public override void Down()
        {
            
        }
    }
}
