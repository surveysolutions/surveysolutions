using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201804201634)]
    public class M201804201634_RemoveHasErrorsPropery : Migration
    {
        public override void Up()
        {
            Delete.Column("errorscount").FromTable("interviewsummaries");
        }

        public override void Down()
        {
            Create.Column("errorscount").OnTable("interviewsummaries").AsInt32().SetExistingRowsTo(0).NotNullable();
        }
    }
}
