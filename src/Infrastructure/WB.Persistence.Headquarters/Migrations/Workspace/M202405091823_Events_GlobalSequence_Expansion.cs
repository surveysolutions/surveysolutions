using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202405091823)]
    public class M202405091823_Events_GlobalSequence_Expansion : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE events ALTER COLUMN globalsequence TYPE bigint USING globalsequence::bigint;");
        }
    }
}
