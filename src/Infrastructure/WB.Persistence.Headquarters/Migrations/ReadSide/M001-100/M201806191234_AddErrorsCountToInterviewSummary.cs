using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201806191234)]
    [Localizable(false)]
    public class M201806191234_AddErrorsCountToInterviewSummary : Migration
    {
        public override void Up()
        {
            Alter.Table("interviewsummaries")
                .AddColumn("errorscount").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            Delete.Column("errorscount").FromTable("interviewsummaries");
        }
    }
}
