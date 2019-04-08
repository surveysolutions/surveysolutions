using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904081532)]
    public class M201904081532_ChangeTypeOfSharedPerson : AutoReversingMigration
    {
        public override void Up()
        {
            Alter.Column("userid").OnTable("sharedpersons").AsString();
            Create.PrimaryKey("sharedpersons_pk").OnTable("sharedpersons").Columns("questionnaireid", "userid");
        }
    }
}
