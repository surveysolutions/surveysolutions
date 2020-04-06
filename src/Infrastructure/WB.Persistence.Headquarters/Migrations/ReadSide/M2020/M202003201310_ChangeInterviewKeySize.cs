using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_03_20_13_10)]
    public class M202003201310_ChangeInterviewKeySize : Migration
    {
        public override void Up()
        {
            Alter.Column("key").OnTable("interviewsummaries").AsString();
        }

        public override void Down()
        {
            Alter.Column("key").OnTable("interviewsummaries").AsString(12);
        }
    }
}
