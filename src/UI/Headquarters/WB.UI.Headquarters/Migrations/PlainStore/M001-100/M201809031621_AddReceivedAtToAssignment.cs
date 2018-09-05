using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(201809031621)]
    public class M201809031621_AddReceivedAtToAssignment : Migration
    {
        public override void Up()
        {
            Create.Column("receivedbytabletatutc").OnTable("assignments").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Column("receivedbytabletatutc").FromTable("assignments");
        }
    }
}
