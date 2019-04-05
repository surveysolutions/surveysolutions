using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201904051548)]
    [Localizable(false)]
    public class M201904051548_Add_Id_Column_to_Interview_Geo_Answers_Table : Migration
    {
        public override void Up()
        {
            var tableName = "interview_geo_answers";
            var primaryKeyName = "pk_interview_geo_answers";

            Delete.PrimaryKey(primaryKeyName).FromTable(tableName);
            Create.Column("id").OnTable(tableName).AsString().Nullable();
            Execute.Sql("UPDATE readside.interview_geo_answers " +
                        "SET \"id\" = trim(interviewid || '-' || translate(questionid::text, '-', '') || '-' || rostervector, '-')");
            Create.PrimaryKey(primaryKeyName).OnTable(tableName).Column("id");
            Create.UniqueConstraint("uc_interview_geo_answers").OnTable(tableName)
                .Columns("interviewid", "questionid", "rostervector");
            Alter.Column("id").OnTable(tableName).AsString().NotNullable();
        }

        public override void Down()
        {
        }
    }
}
