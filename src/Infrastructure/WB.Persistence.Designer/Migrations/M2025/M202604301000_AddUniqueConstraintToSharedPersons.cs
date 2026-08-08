using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_04_30_10_00)]
    public class M202604301000_AddUniqueConstraintToSharedPersons : ForwardOnlyMigration
    {
        public override void Up()
        {
            // Remove duplicate shared persons, keeping the record with the lowest id
            Execute.Sql(@"
                DELETE FROM plainstore.sharedpersons
                WHERE id NOT IN (
                    SELECT MIN(id)
                    FROM plainstore.sharedpersons
                    GROUP BY questionnaireid, userid
                )
            ");

            Create.UniqueConstraint("sharedpersons_questionnaireid_userid_unique")
                .OnTable("sharedpersons")
                .Columns("questionnaireid", "userid");
        }
    }
}
