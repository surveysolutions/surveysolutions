using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904081532)]
    public class M201904081532_ChangeTypeOfSharedPerson : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("id").OnTable("sharedpersons").AsInt32().Identity().PrimaryKey();
        }
    }
}