using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201905231313)]
    public class M201905231313_FixPublicIdForQuestionnaireList : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"update plainstore.questionnairelistviewitems 
                        set publicid = id::uuid
                        where publicid is null");
        }

        public override void Down()
        {
        }
    }
}
