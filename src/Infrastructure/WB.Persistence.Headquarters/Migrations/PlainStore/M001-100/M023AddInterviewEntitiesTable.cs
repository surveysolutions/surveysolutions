using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(23)]
    public class M023AddInterviewEntitiesTable : Migration
    {
        public override void Up()
        {
            Create.Table("interviewentities")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("interviewid").AsGuid()
                .WithColumn("questionid").AsGuid()
                .WithColumn("rostervector").AsCustom("int[]").Nullable()
                .WithColumn("hasflag").AsBoolean().WithDefaultValue(false);
            
            Create.Index("interviewentities_interviewId")
                .OnTable("interviewentities")
                .OnColumn("interviewid");
        }

        public override void Down()
        {
            Delete.Table("interviewentities");
            Delete.Index("interviewentities_interviewId");
        }
    }
}