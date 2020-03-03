using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(202002281347)]
    public class M202002281347_ExtendSyncLogWithException : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("actionexceptionmessage").OnTable("synchronizationlog").AsString().Nullable();
            Create.Column("actionexceptiontype").OnTable("synchronizationlog").AsString().Nullable();
        }
    }
}
