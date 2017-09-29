using FluentMigrator;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(201709291647)]
    public class M201709291647_RemoveQuestionnaireQuestionsInfos : Migration
    {
        public override void Up()
        {
            Delete.Table("questionnairequestionsinfos");
        }

        public override void Down()
        {
            
        }
    }
}