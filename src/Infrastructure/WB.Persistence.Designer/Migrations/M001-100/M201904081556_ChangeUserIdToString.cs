using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904081556)]
    public class M201904081556_ChangeUserIdToString : AutoReversingMigration
    {
        public override void Up()
        {
            Alter.Column("createdby").OnTable("questionnairelistviewitems").AsString();
        }
    }
}
