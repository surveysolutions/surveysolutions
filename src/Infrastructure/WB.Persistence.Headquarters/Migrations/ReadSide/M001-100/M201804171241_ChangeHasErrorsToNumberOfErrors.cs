using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201804171241)]
    public class M201804171241_ChangeHasErrorsToNumberOfErrors : Migration
    {
        public override void Up()
        {
            Create.Column("errorscount").OnTable("interviewsummaries").AsInt32().SetExistingRowsTo(0).NotNullable();
            Delete.Column("haserrors").FromTable("interviewsummaries");
        }

        public override void Down()
        {
            Create.Column("haserrors").OnTable("interviewsummaries").AsBoolean().SetExistingRowsTo(false).Nullable();
            Delete.Column("errorscount").FromTable("interviewsummaries");
        }
    }
}
