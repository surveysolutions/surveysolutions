using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904101227)]
    public class M201904101227_ChangePKToSerial : Migration
    {
        public override void Up()
        {
            this.Execute.Sql(@"CREATE SEQUENCE plainstore.questionnairechangereferences_pk;
ALTER TABLE plainstore.questionnairechangereferences ALTER COLUMN id SET DEFAULT nextval('plainstore.questionnairechangereferences_pk');
ALTER SEQUENCE plainstore.questionnairechangereferences_pk OWNED BY plainstore.questionnairechangereferences.id;
SELECT setval('plainstore.questionnairechangereferences_pk', COALESCE(max(id), 1)) FROM plainstore.questionnairechangereferences;
            ");
        }

        public override void Down()
        {
        }
    }
}
