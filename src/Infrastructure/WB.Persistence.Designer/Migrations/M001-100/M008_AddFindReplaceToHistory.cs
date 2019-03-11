using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(8)]
    public class M008_AddFindReplaceToHistory : Migration
    {
        public override void Up()
        {
            Create.Index("questionnairechangerecords_questionnaireid")
                    .OnTable("questionnairechangerecords")
                    .OnColumn("questionnaireid");

            Create.Column("targetitemnewtitle")
                .OnTable("questionnairechangerecords")
                .AsString(int.MaxValue)
                .Nullable();
            Create.Column("affectedentriescount")
              .OnTable("questionnairechangerecords")
              .AsInt32()
              .Nullable();
        }

        public override void Down()
        {
            Delete.Column("targetitemnewtitle").FromTable("questionnairechangerecords");
            Delete.Column("affectedentriescount").FromTable("questionnairechangerecords");
            Delete.Index("questionnairechangerecords_questionnaireid").OnTable("questionnairechangerecords");
        }
    }
}