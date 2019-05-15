using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(11)]
    public class M011_AddColumnResultingQuestionnaireDocument : Migration
    {
        public override void Up()
        {
            this.Create.Column("resultingquestionnairedocument")
                .OnTable("questionnairechangerecords")
                .AsString()
                .Nullable();

        }

        public override void Down()
        {
            this.Delete.Column("resultingquestionnairedocument")
                .FromTable("questionnairechangerecords");
        }
    }
}