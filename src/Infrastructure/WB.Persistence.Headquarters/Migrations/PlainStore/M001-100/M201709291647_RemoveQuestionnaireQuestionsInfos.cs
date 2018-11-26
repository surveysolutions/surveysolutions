using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201709291647)]
    public class M201709291647_RemoveQuestionnaireQuestionsInfos : Migration
    {
        public override void Up()
        {
            Execute.Sql("DROP TABLE IF EXISTS questionnairequestionsinfos");
        }

        public override void Down()
        {
            
        }
    }
}
