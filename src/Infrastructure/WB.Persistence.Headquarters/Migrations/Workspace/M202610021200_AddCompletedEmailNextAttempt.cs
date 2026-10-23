using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202610021200)]
    public class M202610021200_AddCompletedEmailNextAttempt : AutoReversingMigration
    {
        public override void Up()
        {
            // Null makes existing records, including previously stranded failures, eligible again.
            Alter.Table("completedemailrecords").AddColumn("nextattemptat").AsDateTime().Nullable();
        }
    }
}

