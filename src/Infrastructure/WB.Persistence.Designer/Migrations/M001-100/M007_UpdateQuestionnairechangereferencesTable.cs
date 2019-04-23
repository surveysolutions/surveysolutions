using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(7)]
    public class M007_UpdateQuestionnairechangereferencesTable : Migration
    {
        public override void Up()
        {
            if (Schema.Table("questionnairechangereferences").Exists())
            {
                if (Schema.Table("questionnairechangereferences").Index("questionnairechangerecords_questionnairechangereferences").Exists())
                {
                    Delete.Index("questionnairechangerecords_questionnairechangereferences").OnTable("questionnairechangereferences");
                }

                if (Schema.Table("questionnairechangereferences").Column("questionnairechangerecord").Exists())
                {
                    Delete.Column("questionnairechangerecord").FromTable("questionnairechangereferences");
                }

                Create.Index("questionnairechangerecords_questionnairechangereferences")
                    .OnTable("questionnairechangereferences")
                    .OnColumn("questionnairechangerecordid");
            }
        }

        public override void Down()
        {
            
        }
    }
}