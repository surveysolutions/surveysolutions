using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201904051737)]
    [Localizable(false)]
    public class M201904051737_Add_Id_Column_to_Interview_Geo_Answers_Table : Migration
    {
        public override void Up()
        {
            var tableName = "interview_geo_answers";

            Delete.PrimaryKey("pk_interview_geo_answers").FromTable(tableName);
            Create.Column("id").OnTable(tableName).AsInt32().NotNullable().PrimaryKey().Identity();
            Create.UniqueConstraint().OnTable(tableName)
                .Columns("interviewid", "questionid", "rostervector");
            Create.PrimaryKey().OnTable(tableName).Column("id");
        }

        public override void Down()
        {
        }
    }
}
