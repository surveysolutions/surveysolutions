using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(2020_04_07_13_34)]
    public class M202004071334_ExtendInvitations : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("lastrejectedinterviewemailid").OnTable("invitations")
                .AsString().Nullable();
            Create.Column("lastrejectedinterviewsentatutc").OnTable("invitations")
                .AsDateTime().Nullable();
        }
    }
}
