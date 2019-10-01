using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201909031601)]
    public class M201909031601_HQUserToAssignment : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("headquarters").OnTable("assignmenttoimport").AsGuid().Nullable();
        }
    }
}
